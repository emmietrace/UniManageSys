using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;
using UniManageSys.Services;
using UniManageSys.ViewModels;

namespace UniManageSys.Controllers
{
    public class TimetableController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITimetableService _timetableService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TimetableController(ApplicationDbContext context, ITimetableService timetableService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _timetableService = timetableService;
            _userManager = userManager;
        }

        // GET: Timetable/Schedule
        [Authorize(Roles = "SuperAdmin,Registrar,HOD")]
        public async Task<IActionResult> Schedule()
        {
            var activeSemester = await _context.Semesters
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.IsActive);

            if (activeSemester == null)
            {
                TempData["ErrorMessage"] = "No active semester found. Please open a semester first.";
                return RedirectToAction("Index", "Home");
            }

            // Populate dropdowns for the UI
            ViewBag.Courses = new SelectList(await _context.Courses.OrderBy(c => c.Code).ToListAsync(), "Id", "Code");

            // Format lecturer names nicely for the dropdown
            var lecturers = await _context.Lecturers.Include(l => l.User).ToListAsync();
            ViewBag.Lecturers = new SelectList(lecturers.Select(l => new { l.Id, Name = l.User?.FullName }), "Id", "Name");

            ViewBag.Venues = new SelectList(await _context.Venues.OrderBy(v => v.Name).ToListAsync(), "Id", "Name");
            ViewBag.ActiveSemester = activeSemester;

            return View();
        }

        // POST: Timetable/Schedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Registrar,HOD")]
        public async Task<IActionResult> Schedule(TimetableEvent newEvent)
        {
            // Ensure the Active Semester is preserved
            var activeSemester = await _context.Semesters.FirstOrDefaultAsync(s => s.IsActive);
            if (activeSemester != null) newEvent.SemesterId = activeSemester.Id;

            if (ModelState.IsValid)
            {
                // Send it to the Conflict Detection Engine
                var result = await _timetableService.AddEventAsync(newEvent);

                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Schedule)); // Refresh page for the next entry
                }

                // If there's a clash, show the engine's error message
                TempData["ErrorMessage"] = result.Message;
            }

            // If we fail, reload the dropdowns
            ViewBag.Courses = new SelectList(await _context.Courses.OrderBy(c => c.Code).ToListAsync(), "Id", "Code", newEvent.CourseId);
            var lecturers = await _context.Lecturers.Include(l => l.User).ToListAsync();
            ViewBag.Lecturers = new SelectList(lecturers.Select(l => new { l.Id, Name = l.User?.FullName }), "Id", "Name", newEvent.LecturerId);
            ViewBag.Venues = new SelectList(await _context.Venues.OrderBy(v => v.Name).ToListAsync(), "Id", "Name", newEvent.VenueId);
            ViewBag.ActiveSemester = activeSemester;

            return View(newEvent);
        }

        // GET: Timetable/MyTimetable
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyTimetable()
        {
            // 1. Get Active Semester
            var activeSemester = await _context.Semesters
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.IsActive);

            if (activeSemester == null) return NotFound("No active semester.");

            // 2. Identify the Student
            var user = await _userManager.GetUserAsync(User);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user!.Id);

            if (student == null) return Unauthorized();

            // 3. Find the courses they are actually registered for
            var registeredCourseIds = await _context.CourseRegistrations
                .Where(cr => cr.StudentId == student.Id
                          && cr.SemesterId == activeSemester.Id
                          && cr.Status == Enums.RegistrationStatus.Approved)
                .Select(cr => cr.CourseId)
                .ToListAsync();

            // 4. Fetch ONLY the timetable events for those specific courses
            var myEvents = await _context.TimetableEvents
                .Include(t => t.Course)
                .Include(t => t.Venue)
                .Include(t => t.Lecturer).ThenInclude(l => l!.User)
                .Where(t => t.SemesterId == activeSemester.Id
                         && registeredCourseIds.Contains(t.CourseId))
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            // 5. Group by Day for the UI
            var groupedSchedule = myEvents
                .GroupBy(t => t.Day)
                .ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new PersonalTimetableViewModel
            {
                ActiveSemester = activeSemester,
                WeeklySchedule = groupedSchedule
            };

            return View(viewModel);
        }
    }
}