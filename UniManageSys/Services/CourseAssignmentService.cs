using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;

namespace UniManageSys.Services
{
    public class CourseAssignmentService : ICourseAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public CourseAssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AssignmentResult> AssignCourseAsync(int lecturerId, int courseId, int semesterId, bool isPrimary)
        {
            // RULE 1: Fail-Fast Duplicate Check
            // Prevent the HOD from assigning the exact same course to the same lecturer twice this semester
            var existingAssignment = await _context.CourseAssignments
                .FirstOrDefaultAsync(ca => ca.LecturerId == lecturerId && ca.CourseId == courseId && ca.SemesterId == semesterId);

            if (existingAssignment != null)
            {
                return new AssignmentResult
                {
                    IsSuccess = false,
                    Message = "This lecturer is already assigned to this course for the selected semester."
                };
            }

            // RULE 2: Validate Data Integrity
            var lecturer = await _context.Lecturers
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == lecturerId);

            var course = await _context.Courses.FindAsync(courseId);

            if (lecturer == null || course == null)
            {
                return new AssignmentResult
                {
                    IsSuccess = false,
                    Message = "Invalid Lecturer or Course selected. Please check the database records."
                };
            }

            // SUCCESS: All rules passed. Create the workload assignment.
            var assignment = new CourseAssignment
            {
                LecturerId = lecturerId,
                CourseId = courseId,
                SemesterId = semesterId,
                IsPrimaryLecturer = isPrimary
            };

            _context.CourseAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return new AssignmentResult
            {
                IsSuccess = true,
                Message = $"Successfully assigned {course.Code} to {lecturer.User?.FullName} (Staff ID: {lecturer.StaffId})."
            };
        }

        public async Task<AssignmentResult> RemoveAssignmentAsync(int assignmentId)
        {
            var assignment = await _context.CourseAssignments.FindAsync(assignmentId);

            if (assignment == null)
            {
                return new AssignmentResult { IsSuccess = false, Message = "Assignment record not found." };
            }

            _context.CourseAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return new AssignmentResult { IsSuccess = true, Message = "Course assignment removed successfully." };
        }
    }
}