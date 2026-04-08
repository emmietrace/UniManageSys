using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;
using UniManageSys.ViewModels;
using UniManageSys.Enums;

namespace UniManageSys.Controllers
{
    [Authorize(Roles = "Lecturer,HOD,SuperAdmin")]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AttendanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Attendance/Index
        public async Task<IActionResult> Index()
        {
            var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
            if (activeSemester == null)
            {
                TempData["ErrorMessage"] = "No active semester found.";
                return RedirectToAction("Index", "Home");
            }

            // Fetch all assignments for the active semester
            var query = _context.CourseAssignments
                .Include(ca => ca.Course)
                .Include(ca => ca.Lecturer).ThenInclude(l => l!.User)
                .Where(ca => ca.SemesterId == activeSemester.Id)
                .AsQueryable();

            // If the user is specifically a Lecturer (and not an Admin), filter the list
            if (User.IsInRole("Lecturer") && !User.IsInRole("SuperAdmin"))
            {
                var user = await _userManager.GetUserAsync(User);
                var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);

                if (lecturer != null)
                {
                    query = query.Where(ca => ca.LecturerId == lecturer.Id);
                }
            }

            ViewBag.ActiveSemester = activeSemester;
            return View(await query.ToListAsync());
        }

        // GET: Attendance/Mark/5 (The ID is the CourseAssignmentId)
        public async Task<IActionResult> Mark(int assignmentId)
        {
            // 1. Fetch the Assignment details
            var assignment = await _context.CourseAssignments
                .Include(ca => ca.Course)
                .Include(ca => ca.Semester)
                .FirstOrDefaultAsync(ca => ca.Id == assignmentId);

            if (assignment == null) return NotFound("Course assignment not found.");

            // 2. Fetch all students APPROVED for this course in this semester
            var registeredStudents = await _context.CourseRegistrations
                .Include(cr => cr.Student).ThenInclude(s => s!.User)
                .Where(cr => cr.CourseId == assignment.CourseId
                          && cr.SemesterId == assignment.SemesterId
                          && cr.Status == Enums.RegistrationStatus.Approved)
                .OrderBy(cr => cr.Student!.MatriculationNumber)
                .ToListAsync();

            // 3. Pack them into the ViewModel
            var viewModel = new TakeAttendanceViewModel
            {
                CourseAssignmentId = assignment.Id,
                CourseCode = assignment.Course!.Code,
                CourseTitle = assignment.Course.Title,
                SessionDate = DateTime.Today,
                Students = registeredStudents.Select(cr => new StudentAttendanceItem
                {
                    StudentId = cr.StudentId,
                    MatricNumber = cr.Student!.MatriculationNumber,
                    FullName = cr.Student.User!.FullName
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Attendance/Mark
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Mark(TakeAttendanceViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Anti-Cheat: Prevent a lecturer from marking the exact same day twice by accident
            bool alreadyMarked = await _context.ClassSessions
                .AnyAsync(cs => cs.CourseAssignmentId == model.CourseAssignmentId
                             && cs.SessionDate.Date == model.SessionDate.Date);

            if (alreadyMarked)
            {
                ModelState.AddModelError("", "Attendance for this specific date has already been recorded.");
                return View(model);
            }

            // 1. Create the Master Session Record
            var newSession = new ClassSession
            {
                CourseAssignmentId = model.CourseAssignmentId,
                SessionDate = model.SessionDate,
                TopicTaught = model.TopicTaught
            };

            _context.ClassSessions.Add(newSession);
            await _context.SaveChangesAsync(); // Save to generate the ClassSession ID

            // 2. Create the individual student records mapped to this new session
            var records = model.Students.Select(s => new AttendanceRecord
            {
                ClassSessionId = newSession.Id,
                StudentId = s.StudentId,
                Status = s.Status,
                Remarks = s.Remarks
            }).ToList();

            _context.AttendanceRecords.AddRange(records);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Attendance saved successfully for {model.Students.Count} students.";
            return RedirectToAction(nameof(Index)); // Or wherever you route your lecturers
        }

        // GET: Attendance/Report/5
        public async Task<IActionResult> Report(int assignmentId)
        {
            // 1. Get the Assignment and Total Classes Held
            var assignment = await _context.CourseAssignments
                .Include(ca => ca.Course)
                .FirstOrDefaultAsync(ca => ca.Id == assignmentId);

            if (assignment == null) return NotFound();

            var totalClasses = await _context.ClassSessions
                .CountAsync(cs => cs.CourseAssignmentId == assignmentId);

            // 2. Prepare the ViewModel
            var report = new AttendanceReportViewModel
            {
                CourseAssignmentId = assignment.Id,
                CourseCode = assignment.Course!.Code,
                CourseTitle = assignment.Course.Title,
                TotalClassesHeld = totalClasses
            };

            // If no classes have been held yet, return the empty report
            if (totalClasses == 0) return View(report);

            // 3. Get all approved students for this course
            var registeredStudents = await _context.CourseRegistrations
                .Include(cr => cr.Student).ThenInclude(s => s!.User)
                .Where(cr => cr.CourseId == assignment.CourseId
                  && cr.SemesterId == assignment.SemesterId
                  && cr.Status == Enums.RegistrationStatus.Approved)
                .Select(cr => cr.Student)
                .ToListAsync();

            // 4. THE THRESHOLD ENGINE: Calculate attendance for each student
            foreach (var student in registeredStudents)
            {
                // Count how many times they were Present OR Excused
                var attendedCount = await _context.AttendanceRecords
                    .Include(ar => ar.ClassSession)
                    .Where(ar => ar.StudentId == student!.Id
                              && ar.ClassSession!.CourseAssignmentId == assignmentId
                              && (ar.Status == AttendanceStatus.Present || ar.Status == AttendanceStatus.Excused)) 
                    .CountAsync();

                // Calculate the percentage
                double percentage = Math.Round(((double)attendedCount / totalClasses) * 100, 1);

                report.StudentSummaries.Add(new StudentAttendanceSummary
                {
                    MatricNumber = student!.MatriculationNumber,
                    FullName = student.User!.FullName,
                    ClassesAttended = attendedCount,
                    AttendancePercentage = percentage
                });
            }

            // Sort alphabetically by Matric Number
            report.StudentSummaries = report.StudentSummaries.OrderBy(s => s.MatricNumber).ToList();

            return View(report);
        }
    }
}