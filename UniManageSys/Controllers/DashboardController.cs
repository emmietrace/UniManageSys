using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;
using UniManageSys.ViewModels;

namespace UniManageSys.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // THE TRAFFIC COP
        public IActionResult Index()
        {
            if (User.IsInRole("SuperAdmin") || User.IsInRole("Registrar"))
                return RedirectToAction(nameof(AdminDashboard));

            if (User.IsInRole("HOD"))
                return RedirectToAction(nameof(AdminDashboard)); // HODs can share a variant of the Admin view

            if (User.IsInRole("Lecturer"))
                return RedirectToAction(nameof(LecturerDashboard));

            if (User.IsInRole("Student"))
                return RedirectToAction(nameof(StudentDashboard));

            return View(); // Fallback generic view
        }

        // --- STUDENT DASHBOARD ---
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Programme)
                .FirstOrDefaultAsync(s => s.UserId == user!.Id);

            var activeSemester = await _context.Semesters.Include(s => s.AcademicYear).FirstOrDefaultAsync(s => s.IsActive);

            if (student == null || activeSemester == null) return NotFound();

            // Fetch their approved registrations to power the existing logic
            var registrations = await _context.CourseRegistrations
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == student.Id && cr.SemesterId == activeSemester.Id)
                .ToListAsync();

            // Fetch today's timetable based ONLY on courses they are approved for
            var today = DateTime.Today.DayOfWeek;
            var todaysClasses = await _context.TimetableEvents
                .Include(t => t.Course)
                .Include(t => t.Venue)
                .Where(t => t.SemesterId == activeSemester.Id
                         && t.Day == today
                         && _context.CourseRegistrations.Any(cr => cr.StudentId == student.Id && cr.CourseId == t.CourseId && cr.Status == Enums.RegistrationStatus.Approved))
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            var vm = new StudentDashboardViewModel
            {
                StudentProfile = student,
                CurrentSemester = activeSemester,
                CurrentRegistrations = registrations,
                TodaysClasses = todaysClasses
            };

            return View(vm);
        }

        // --- LECTURER DASHBOARD ---
        [Authorize(Roles = "Lecturer,HOD")]
        public async Task<IActionResult> LecturerDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var lecturer = await _context.Lecturers.Include(l => l.User).FirstOrDefaultAsync(l => l.UserId == user!.Id);
            var activeSemester = await _context.Semesters.Include(s => s.AcademicYear).FirstOrDefaultAsync(s => s.IsActive);

            if (lecturer == null || activeSemester == null) return NotFound();

            var assignedCourses = await _context.CourseAssignments
                .Include(ca => ca.Course)
                .Where(ca => ca.LecturerId == lecturer.Id && ca.SemesterId == activeSemester.Id)
                .ToListAsync();

            var today = DateTime.Today.DayOfWeek;
            var todaysLectures = await _context.TimetableEvents
                .Include(t => t.Course)
                .Include(t => t.Venue)
                .Where(t => t.SemesterId == activeSemester.Id && t.LecturerId == lecturer.Id && t.Day == today)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            var vm = new LecturerDashboardViewModel
            {
                LecturerProfile = lecturer,
                CurrentSemester = activeSemester,
                AssignedCourses = assignedCourses,
                TodaysLectures = todaysLectures
            };

            // 1. Find the courses assigned to this lecturer for the active semester
            var assignedCourseIds = await _context.CourseAssignments
                .Where(ca => ca.LecturerId == lecturer.Id && ca.SemesterId == activeSemester.Id)
                .Select(ca => ca.CourseId)
                .ToListAsync();

            // 2. Count all approved student registrations for those specific courses
            int totalStudents = await _context.CourseRegistrations
                .Where(cr => assignedCourseIds.Contains(cr.CourseId)
                          && cr.SemesterId == activeSemester.Id
                          && cr.Status == Enums.RegistrationStatus.Approved)
                .CountAsync();

            // Add it to your ViewModel
            vm.TotalStudents = totalStudents;

            return View(vm);
        }

        // --- ADMIN DASHBOARD ---
        [Authorize(Roles = "SuperAdmin,Registrar,HOD")]
        public async Task<IActionResult> AdminDashboard()
        {
            var activeSemester = await _context.Semesters.Include(s => s.AcademicYear).FirstOrDefaultAsync(s => s.IsActive);

            var vm = new AdminDashboardViewModel
            {
                ActiveSemester = activeSemester,
                TotalStudents = await _context.Students.CountAsync(),
                TotalLecturers = await _context.Lecturers.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                PendingRegistrations = await _context.CourseRegistrations.CountAsync(cr => cr.Status == Enums.RegistrationStatus.Pending)
            };
            return View(vm);
        }
    }
}