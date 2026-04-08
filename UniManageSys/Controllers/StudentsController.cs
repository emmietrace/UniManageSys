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
    [Authorize(Roles = "SuperAdmin,Registrar,HOD")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMatriculationService _matricService;

        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMatriculationService matricService)
        {
            _context = context;
            _userManager = userManager;
            _matricService = matricService;
        }

        // 1. ENROLLMENT (ADMISSIONS)
        
        // GET: Students/Enroll
        public async Task<IActionResult> Enroll()
        {
            var progQuery = _context.Programmes.AsQueryable();

            // DATA ISOLATION: HOD sees only programmes belonging to their department
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                progQuery = progQuery.Where(p => p.DepartmentId == hodProfile!.DepartmentId);
            }

            ViewData["ProgrammeId"] = new SelectList(await progQuery.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Students/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(StudentEnrollmentViewModel model)
        {
            // SECURITY CHECK: Ensure HOD isn't hacking the form to admit to other departments
            if (User.IsInRole("HOD"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == currentUser!.Id);

                var selectedProgramme = await _context.Programmes.FindAsync(model.ProgrammeId);
                if (selectedProgramme?.DepartmentId != hodProfile!.DepartmentId)
                {
                    TempData["ErrorMessage"] = "Access Denied: You cannot admit students to other departments.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true
                };

                // Default password for new students
                var result = await _userManager.CreateAsync(user, "Student@123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");

                    string newMatricNumber = await _matricService.GenerateMatricNumberAsync(model.EnrollmentYear, model.ProgrammeId);

                    var student = new Student
                    {
                        UserId = user.Id,
                        ProgrammeId = model.ProgrammeId,
                        EnrollmentYear = model.EnrollmentYear,
                        MatriculationNumber = newMatricNumber,
                        CurrentLevel = model.StartingLevel, // Handles Direct Entry/Transfers
                        Status = Enums.StudentStatus.Active
                    };

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Student Enrolled Successfully! Matric No: {newMatricNumber}";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we fail, reload the dropdown safely
            var pQuery = _context.Programmes.AsQueryable();
            if (User.IsInRole("HOD"))
            {
                var u = await _userManager.GetUserAsync(User);
                var h = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == u!.Id);
                pQuery = pQuery.Where(p => p.DepartmentId == h!.DepartmentId);
            }

            ViewData["ProgrammeId"] = new SelectList(await pQuery.ToListAsync(), "Id", "Name", model.ProgrammeId);
            return View(model);
        }

        // 2. DIRECTORY & PROFILES
        
        // GET: Students/Index
        public async Task<IActionResult> Index()
        {
            var query = _context.Students
                .Include(s => s.User)
                .Include(s => s.Programme)
                    .ThenInclude(p => p.Department)
                .AsQueryable();

            // DATA ISOLATION: HOD only sees their own students
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                query = query.Where(s => s.Programme.DepartmentId == lecturer!.DepartmentId);
            }

            return View(await query.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Programme)
                    .ThenInclude(p => p.Department)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            // SECURITY CHECK
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                if (student.Programme.DepartmentId != hodProfile?.DepartmentId)
                {
                    TempData["ErrorMessage"] = "Access Denied: This student belongs to a different department.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Programme)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            // SECURITY CHECK
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                if (student.Programme.DepartmentId != hodProfile?.DepartmentId)
                {
                    TempData["ErrorMessage"] = "Access Denied: This student belongs to a different department.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var pQuery = _context.Programmes.AsQueryable();
            if (User.IsInRole("HOD"))
            {
                var u = await _userManager.GetUserAsync(User);
                var h = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == u!.Id);
                pQuery = pQuery.Where(p => p.DepartmentId == h!.DepartmentId);
            }

            ViewBag.Programmes = new SelectList(await pQuery.ToListAsync(), "Id", "Name", student.ProgrammeId);
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string FirstName, string LastName, int CurrentLevel, int ProgrammeId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Programme)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            // SECURITY CHECK
            if (User.IsInRole("HOD"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == currentUser!.Id);
                if (student.Programme.DepartmentId != hodProfile?.DepartmentId)
                {
                    return Unauthorized();
                }
            }

            if (ModelState.IsValid)
            {
                student.User!.FirstName = FirstName;
                student.User.LastName = LastName;
                student.CurrentLevel = CurrentLevel;
                student.ProgrammeId = ProgrammeId;

                _context.Update(student);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Student profile updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Programmes = new SelectList(_context.Programmes, "Id", "Name", student.ProgrammeId);
            return View(student);
        }

        // 3. ACADEMICS & REGISTRATIONS
        
        // GET: Students/AcademicRecord/5
        public async Task<IActionResult> AcademicRecord(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Programme)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            // SECURITY CHECK
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                if (student.Programme.DepartmentId != hodProfile?.DepartmentId)
                {
                    TempData["ErrorMessage"] = "Access Denied.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var records = await _context.CourseRegistrations
                .Include(cr => cr.Course)
                .Include(cr => cr.Semester)
                .Include(cr => cr.Result)
                .Where(cr => cr.StudentId == id && cr.Status == Enums.RegistrationStatus.Approved)
                .OrderByDescending(cr => cr.Semester!.AcademicYearId)
                .ToListAsync();

            ViewBag.Student = student;
            return View(records);
        }

        // GET: Students/PendingApprovals
        public async Task<IActionResult> PendingApprovals()
        {
            var query = _context.CourseRegistrations
                .Include(cr => cr.Student).ThenInclude(s => s.User)
                .Include(cr => cr.Student.Programme).ThenInclude(p => p.Department)
                .Include(cr => cr.Course)
                .Include(cr => cr.Semester)
                .Where(cr => cr.Status == Enums.RegistrationStatus.Pending);

            // DATA ISOLATION: HOD only sees pending approvals for their department
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);

                query = query.Where(cr => cr.Student.Programme.DepartmentId == lecturer!.DepartmentId);
            }

            var pending = await query.ToListAsync();
            return View(pending);
        }

        // POST: Students/ApproveRegistration
        [HttpPost]
        public async Task<IActionResult> ApproveRegistration(int id)
        {
            var reg = await _context.CourseRegistrations.FindAsync(id);
            if (reg != null)
            {
                reg.Status = Enums.RegistrationStatus.Approved;
                reg.ApprovedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Registration approved successfully.";
            }
            return RedirectToAction(nameof(PendingApprovals));
        }

        // POST: Students/DropApprovedCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DropApprovedCourse(int registrationId, int studentId)
        {
            var registration = await _context.CourseRegistrations.FindAsync(registrationId);

            if (registration != null)
            {
                _context.CourseRegistrations.Remove(registration);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Administrative Override: Course successfully dropped and credits refunded to student.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not find the specified course registration.";
            }

            return RedirectToAction(nameof(AcademicRecord), new { id = studentId });
        }
    }
}