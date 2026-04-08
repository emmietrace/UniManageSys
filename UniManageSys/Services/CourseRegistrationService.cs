using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Enums;
using UniManageSys.Models;

namespace UniManageSys.Services
{
    public class CourseRegistrationService : ICourseRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private const int MAX_CREDIT_UNITS = 24; // University-wide business rule

        public CourseRegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RegistrationResult> RegisterCourseAsync(int studentId, int courseId, int semesterId)
        {
            // RULE 1: No Double Registration
            var existingReg = await _context.CourseRegistrations
                .FirstOrDefaultAsync(cr => cr.StudentId == studentId && cr.CourseId == courseId && cr.SemesterId == semesterId);

            if (existingReg != null)
                return new RegistrationResult { IsSuccess = false, Message = "You have already registered for this course this semester." };

            // Fetch the course with its prerequisites included
            var course = await _context.Courses
                .Include(c => c.Prerequisites)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return new RegistrationResult { IsSuccess = false, Message = "Course not found." };

            // RULE 2: Credit Unit Limit Enforcement
            var currentRegistrations = await _context.CourseRegistrations
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == studentId && cr.SemesterId == semesterId && cr.Status != RegistrationStatus.Dropped)
                .ToListAsync();

            int currentTotalCredits = currentRegistrations.Sum(cr => cr.Course!.CreditUnits);

            if (currentTotalCredits + course.CreditUnits > MAX_CREDIT_UNITS)
            {
                return new RegistrationResult
                {
                    IsSuccess = false,
                    Message = $"Registration blocked. Adding {course.CreditUnits} units exceeds the maximum limit of {MAX_CREDIT_UNITS}."
                };
            }

            // RULE 3: Prerequisite Enforcement
            if (course.Prerequisites.Any())
            {
                // Find all courses the student has successfully taken in the past
                var passedCourses = await _context.CourseRegistrations
                    .Where(cr => cr.StudentId == studentId && cr.Status == RegistrationStatus.Approved)
                    .Select(cr => cr.CourseId)
                    .ToListAsync();

                foreach (var prereq in course.Prerequisites)
                {
                    if (!passedCourses.Contains(prereq.PrerequisiteId))
                    {
                        // Look up the specific code so we can tell the user exactly what they are missing
                        var prereqCourse = await _context.Courses.FindAsync(prereq.PrerequisiteId);
                        return new RegistrationResult
                        {
                            IsSuccess = false,
                            Message = $"Missing prerequisite: You must complete {prereqCourse?.Code} before taking this course."
                        };
                    }
                }
            }

            // SUCCESS: All rules passed. Create the workflow draft!
            var newRegistration = new CourseRegistration
            {
                StudentId = studentId,
                CourseId = courseId,
                SemesterId = semesterId,
                Status = RegistrationStatus.Draft // Waiting for HOD/Adviser Approval!
            };

            _context.CourseRegistrations.Add(newRegistration);
            await _context.SaveChangesAsync();

            return new RegistrationResult { IsSuccess = true, Message = "Course added to your cart. Don't forget to submit!" };
        }

        public async Task<RegistrationResult> SubmitRegistrationAsync(int studentId, int semesterId)
        {
            // Find all courses sitting in the student's cart
            var drafts = await _context.CourseRegistrations
                .Where(cr => cr.StudentId == studentId && cr.SemesterId == semesterId && cr.Status == RegistrationStatus.Draft)
                .ToListAsync();

            if (!drafts.Any())
                return new RegistrationResult { IsSuccess = false, Message = "No draft courses to submit." };

            // Flip them all to Pending (Waiting for HOD)
            foreach (var draft in drafts)
            {
                draft.Status = RegistrationStatus.Pending;
                draft.RegistrationDate = DateTime.UtcNow; // Stamp the exact time they clicked submit
            }

            await _context.SaveChangesAsync();
            return new RegistrationResult { IsSuccess = true, Message = "Registration successfully submitted to the HOD for approval." };
        }
    }
}