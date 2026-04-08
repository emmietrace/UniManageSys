using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;
using UniManageSys.Services; // Ensure this is here
using UniManageSys.ViewModels;

namespace UniManageSys.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class TeachingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGradingService _gradingService; // 1. Added Service

        public TeachingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IGradingService gradingService) // 2. Updated Constructor
        {
            _context = context;
            _userManager = userManager;
            _gradingService = gradingService;
        }

        // GET: Teaching/MyClasses
        public async Task<IActionResult> MyClasses()
        {
            var user = await _userManager.GetUserAsync(User);
            var lecturer = await _context.Lecturers
                .Include(l => l.Department)
                .FirstOrDefaultAsync(l => l.UserId == user!.Id);

            if (lecturer == null) return NotFound("Lecturer profile missing.");

            var activeSemester = await _context.Semesters
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.IsActive);

            if (activeSemester == null)
            {
                TempData["ErrorMessage"] = "The academic semester is currently closed.";
                return View(new LecturerClassesViewModel { LecturerProfile = lecturer });
            }

            var myAssignments = await _context.CourseAssignments
                .Include(ca => ca.Course)
                .Where(ca => ca.LecturerId == lecturer.Id && ca.SemesterId == activeSemester.Id)
                .ToListAsync();

            var viewModel = new LecturerClassesViewModel
            {
                LecturerProfile = lecturer,
                ActiveSemester = activeSemester,
                AssignedCourses = myAssignments
            };

            return View(viewModel);
        }

        // 3. ADDED: GET: Teaching/Gradebook/5
        public async Task<IActionResult> Gradebook(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);

            // Fetch the specific assignment
            var assignment = await _context.CourseAssignments
                .Include(ca => ca.Course)
                .Include(ca => ca.Semester).ThenInclude(s => s.AcademicYear)
                .FirstOrDefaultAsync(ca => ca.Id == id && ca.LecturerId == lecturer!.Id);

            if (assignment == null) return NotFound("Class not found.");

            // Fetch students with Approved status
            var roster = await _context.CourseRegistrations
                .Include(cr => cr.Student).ThenInclude(s => s.User)
                .Include(cr => cr.Result)
                .Where(cr => cr.CourseId == assignment.CourseId
                          && cr.SemesterId == assignment.SemesterId
                          && cr.Status == Enums.RegistrationStatus.Approved)
                .OrderBy(cr => cr.Student!.MatriculationNumber)
                .ToListAsync();

            var viewModel = new GradebookViewModel
            {
                Assignment = assignment,
                RegisteredStudents = roster
            };

            return View(viewModel);
        }

        // 4. ADDED: POST: Teaching/SaveScore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveScore(int registrationId, int assignmentId, decimal caScore, decimal examScore)
        {
            var user = await _userManager.GetUserAsync(User);
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);

            if (lecturer == null) return Unauthorized();

            // Fire the Nigerian Grading Engine
            var result = await _gradingService.ProcessScoreAsync(registrationId, lecturer.Id, caScore, examScore);

            if (result.IsSuccess) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Gradebook), new { id = assignmentId });
        }

        // GET: Teaching/Roster/5
        public async Task<IActionResult> Roster(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);

            var assignment = await _context.CourseAssignments
                .Include(ca => ca.Course)
                .Include(ca => ca.Semester)
                .FirstOrDefaultAsync(ca => ca.Id == id && ca.LecturerId == lecturer!.Id);

            if (assignment == null) return NotFound();

            var students = await _context.CourseRegistrations
                .Include(cr => cr.Student).ThenInclude(s => s.User)
                .Where(cr => cr.CourseId == assignment.CourseId
                          && cr.SemesterId == assignment.SemesterId
                          && cr.Status == Enums.RegistrationStatus.Approved)
                .OrderBy(cr => cr.Student!.MatriculationNumber)
                .ToListAsync();

            var viewModel = new GradebookViewModel // We can reuse the same ViewModel!
            {
                Assignment = assignment,
                RegisteredStudents = students
            };

            return View(viewModel);
        }
    }
}