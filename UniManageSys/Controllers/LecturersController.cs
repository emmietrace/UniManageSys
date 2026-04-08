using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;
using UniManageSys.ViewModels;
using UniManageSys.Services;

namespace UniManageSys.Controllers
{
    // Only high-level admins can hire staff
    [Authorize(Roles = "SuperAdmin,Registrar,HOD")]
    public class LecturersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICourseAssignmentService _assignmentService;

        public LecturersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ICourseAssignmentService assignmentService)
        {
            _context = context;
            _userManager = userManager;
            _assignmentService = assignmentService;
        }

        // GET: Lecturers/Onboard
        public async Task<IActionResult> Onboard()
        {
            var deptQuery = _context.Departments.AsQueryable();

            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);

                // HOD sees only their department
                deptQuery = deptQuery.Where(d => d.Id == hodProfile!.DepartmentId);
            }

            ViewData["DepartmentId"] = new SelectList(await deptQuery.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Lecturers/Onboard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onboard(LecturerOnboardViewModel model)
        {
            // SECURITY: Ensure HOD isn't hacking the form to assign to another department
            if (User.IsInRole("HOD"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == currentUser!.Id);
                if (model.DepartmentId != hodProfile!.DepartmentId)
                {
                    TempData["ErrorMessage"] = "Access Denied: You can only onboard staff to your own department.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                if (await _context.Lecturers.AnyAsync(l => l.StaffId == model.StaffId))
                {
                    ModelState.AddModelError("StaffId", "This Staff ID is already assigned to another lecturer.");

                    var deptQuery = _context.Departments.AsQueryable();
                    if (User.IsInRole("HOD"))
                    {
                        var u = await _userManager.GetUserAsync(User);
                        var h = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == u!.Id);
                        deptQuery = deptQuery.Where(d => d.Id == h!.DepartmentId);
                    }
                    ViewData["DepartmentId"] = new SelectList(await deptQuery.ToListAsync(), "Id", "Name", model.DepartmentId);
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "Staff@123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Lecturer");

                    var lecturer = new Lecturer
                    {
                        UserId = user.Id,
                        StaffId = model.StaffId,
                        Rank = model.Rank,
                        DepartmentId = model.DepartmentId
                    };

                    _context.Lecturers.Add(lecturer);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Lecturer {model.FirstName} {model.LastName} successfully onboarded!";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", model.DepartmentId);
            return View(model);
        }

        // Placeholder for the Staff Directory
        /// GET: Lecturers/Index
        public async Task<IActionResult> Index()
        {
            var query = _context.Lecturers
                .Include(l => l.User)
                .Include(l => l.Department)
                .AsQueryable();

            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);

                // Filter the list to only show the HOD's department staff
                query = query.Where(l => l.DepartmentId == hodProfile!.DepartmentId);
            }

            var lecturers = await query
                .OrderBy(l => l.Department!.Name)
                .ThenBy(l => l.User!.LastName)
                .ToListAsync();

            return View(lecturers);
        }

        // GET: Lecturers/Workload/5
        public async Task<IActionResult> Workload(int id)
        {
            // 1. Fetch the Lecturer and their current assignments
            var lecturer = await _context.Lecturers
                .Include(l => l.User)
                .Include(l => l.Department)
                .Include(l => l.CourseAssignments)
                    .ThenInclude(ca => ca.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecturer == null) return NotFound();

            // 2. Get the Active Semester
            var activeSemester = await _context.Semesters
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.IsActive);

            if (activeSemester == null)
            {
                TempData["ErrorMessage"] = "No active semester found. Cannot assign workload.";
                return RedirectToAction(nameof(Index));
            }

            // 3. Get all courses belonging to this lecturer's department
            var departmentCourses = await _context.Courses
                .Where(c => c.DepartmentId == lecturer.DepartmentId)
                .OrderBy(c => c.Level).ThenBy(c => c.Code)
                .ToListAsync();

            var viewModel = new LecturerWorkloadViewModel
            {
                LecturerProfile = lecturer,
                ActiveSemester = activeSemester,
                AvailableCourses = new SelectList(departmentCourses, "Id", "Code") // Shows "CSC101", passes the ID
            };

            return View(viewModel);
        }

        // POST: Lecturers/AssignCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCourse(int lecturerId, int selectedCourseId, int semesterId, bool isPrimaryLecturer)
        {
            // Trigger the Business Logic Engine
            var result = await _assignmentService.AssignCourseAsync(lecturerId, selectedCourseId, semesterId, isPrimaryLecturer);

            if (result.IsSuccess) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            // Redirect back to the workload page to see the update
            return RedirectToAction(nameof(Workload), new { id = lecturerId });
        }

        // POST: Lecturers/RemoveAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignment(int assignmentId, int lecturerId)
        {
            var result = await _assignmentService.RemoveAssignmentAsync(assignmentId);

            if (result.IsSuccess) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Workload), new { id = lecturerId });
        }

        // POST: Lecturers/AssignHOD
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Registrar")] // Only top-level admins can appoint HODs
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignHOD(int lecturerId, int departmentId)
        {
            // 1. Find the Lecturer and their User account
            var lecturer = await _context.Lecturers
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == lecturerId);

            if (lecturer == null) return NotFound();

            // 2. Find the Department
            var dept = await _context.Departments.FindAsync(departmentId);
            if (dept == null) return NotFound();

            // 3. SECURE THE IDENTITY ROLE
            // Give them the HOD role if they don't have it
            var user = await _userManager.FindByIdAsync(lecturer.UserId);
            if (!await _userManager.IsInRoleAsync(user!, "HOD"))
            {
                await _userManager.AddToRoleAsync(user!, "HOD");
            }

            // 4. Update the Department record
            dept.HODId = lecturerId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{lecturer.User?.FullName} is now the HOD of {dept.Name}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Lecturers/EmergencyReset?email=thelecturer@unisystem.edu
        [Authorize(Roles = "SuperAdmin,Registrar")]
        public async Task<IActionResult> EmergencyReset(string email)
        {
            if (string.IsNullOrEmpty(email)) return Content("Please provide an email in the URL.");

            // 1. Find the locked out user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Content($"No user found with email: {email}");

            // 2. Generate a system override token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 3. Force the password reset using the token
            var result = await _userManager.ResetPasswordAsync(user, resetToken, "Staff@123!");

            if (result.Succeeded)
            {
                return Content($"SUCCESS! The password for {email} has been reset back to: Staff@123!");
            }

            // If it fails, print out why
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Content($"Failed to reset: {errors}");
        }
    }
}