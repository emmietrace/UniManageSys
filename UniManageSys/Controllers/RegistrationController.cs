using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;
using UniManageSys.Services;
using UniManageSys.ViewModels;

namespace UniManageSys.Controllers
{
    // ONLY Students can access this portal
    [Authorize(Roles = "Student")]
    public class RegistrationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourseRegistrationService _registrationService;

        public RegistrationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICourseRegistrationService registrationService)
        {
            _context = context;
            _userManager = userManager;
            _registrationService = registrationService;
        }

        // GET: Registration/Portal
        public async Task<IActionResult> Portal()
        {
            // 1. Identify the logged-in student
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students
                .Include(s => s.Programme)
                .FirstOrDefaultAsync(s => s.UserId == user!.Id);

            if (student == null) return NotFound("Student profile missing.");

            // 2. Find the currently active semester
            var activeSemester = await _context.Semesters
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.IsActive);

            if (activeSemester == null)
            {
                TempData["ErrorMessage"] = "Registration is currently closed. No active semester found.";
                return View(new StudentDashboardViewModel { StudentProfile = student });
            }

            // 3. Load their current registrations
            var myRegistrations = await _context.CourseRegistrations
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == student.Id && cr.SemesterId == activeSemester.Id)
                .ToListAsync();

            // 4. Load available courses for their Department & Level
            // (Excluding ones they've already registered for)
            var registeredCourseIds = myRegistrations.Select(r => r.CourseId).ToList();

            var availableCourses = await _context.Courses
                .Include(c => c.Prerequisites)
                .ThenInclude(p => p.Prerequisite)
                .Where(c => c.DepartmentId == student.Programme!.DepartmentId
                         && c.Level <= student.CurrentLevel
                         && !registeredCourseIds.Contains(c.Id))
                .ToListAsync();

            // 5. Package it all up for the View
            var viewModel = new StudentDashboardViewModel
            {
                StudentProfile = student,
                CurrentSemester = activeSemester,
                AvailableCourses = availableCourses,
                CurrentRegistrations = myRegistrations
            };

            return View(viewModel);
        }

        // POST: Registration/AddCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user!.Id);
            var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);

            if (student == null || activeSemester == null) return BadRequest();

            // Trigger our Iron-Clad Business Logic Engine!
            var result = await _registrationService.RegisterCourseAsync(student.Id, courseId, activeSemester.Id);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message; // E.g., "Missing Prerequisite" or "Credit Limit Exceeded"
            }

            return RedirectToAction(nameof(Portal));
        }
        // POST: Registration/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit()
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user!.Id);
            var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);

            if (student == null || activeSemester == null) return BadRequest();

            // Fire the submission engine
            var result = await _registrationService.SubmitRegistrationAsync(student.Id, activeSemester.Id);

            if (result.IsSuccess) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Portal));
        }
        // POST: CourseRegistration/DropCourse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DropCourse(int id)
        {
            // Find the specific registration record the user wants to drop
            var registration = await _context.CourseRegistrations.FindAsync(id);

            // Security Check: Only allow dropping if it's in Draft or Pending state.
            // If it's already Approved, they shouldn't be able to drop it themselves!
            if (registration != null && registration.Status != Enums.RegistrationStatus.Approved)
            {
                _context.CourseRegistrations.Remove(registration);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Course successfully removed from your registration list.";
            }
            else if (registration != null && registration.Status == Enums.RegistrationStatus.Approved)
            {
                TempData["ErrorMessage"] = "You cannot drop an approved course. Please see your HOD.";
            }
            else
            {
                TempData["ErrorMessage"] = "Course registration not found.";
            }

            // Redirect them right back to the Course Registration portal so they can keep working
            return RedirectToAction("Portal");
        }

        // POST: Registration/ResetCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetCart()
        {
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user!.Id);
            var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);

            if (student == null || activeSemester == null) return NotFound();

            // Find ALL unapproved courses (Drafts or Pending) for this semester
            var unapprovedRegistrations = await _context.CourseRegistrations
                .Where(cr => cr.StudentId == student.Id
                          && cr.SemesterId == activeSemester.Id
                          && cr.Status != Enums.RegistrationStatus.Approved)
                .ToListAsync();

            if (unapprovedRegistrations.Any())
            {
                _context.CourseRegistrations.RemoveRange(unapprovedRegistrations);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your registration cart has been reset. You can now select new courses.";
            }
            else
            {
                TempData["ErrorMessage"] = "You have no unapproved courses to reset.";
            }

            // Redirect back to the Portal so they see the empty cart instantly
            return RedirectToAction(nameof(Portal));
        }
    }
}