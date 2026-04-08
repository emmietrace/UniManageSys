using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;

namespace UniManageSys.Controllers
{
    [Authorize(Roles = "SuperAdmin,Registrar,HOD")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // NEW CONSTRUCTOR WITH USER MANAGER
        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var query = _context.Courses.Include(c => c.Department).AsQueryable();

            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                query = query.Where(c => c.DepartmentId == hodProfile!.DepartmentId);
            }

            var courses = await query.OrderBy(c => c.Level).ThenBy(c => c.Code).ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> Create()
        {
            var deptQuery = _context.Departments.AsQueryable();

            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                deptQuery = deptQuery.Where(d => d.Id == hodProfile!.DepartmentId);
            }

            ViewBag.Departments = new SelectList(await deptQuery.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                if (course.DepartmentId != hodProfile!.DepartmentId)
                {
                    TempData["ErrorMessage"] = "You can only create courses for your own department.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                course.Code = course.Code.ToUpper();

                if (await _context.Courses.AnyAsync(c => c.Code == course.Code))
                {
                    ModelState.AddModelError("Code", "A course with this code already exists.");
                    ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", course.DepartmentId);
                    return View(course);
                }

                _context.Add(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{course.Code} has been successfully added to the curriculum.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", course.DepartmentId);
            return View(course);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var deptQuery = _context.Departments.AsQueryable();
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);

                if (course.DepartmentId != hodProfile!.DepartmentId)
                {
                    TempData["ErrorMessage"] = "Access Denied.";
                    return RedirectToAction(nameof(Index));
                }
                deptQuery = deptQuery.Where(d => d.Id == hodProfile.DepartmentId);
            }

            ViewBag.Departments = new SelectList(await deptQuery.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", course.DepartmentId);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.Id) return NotFound();

            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                if (course.DepartmentId != hodProfile!.DepartmentId)
                {
                    return Unauthorized();
                }
            }

            if (ModelState.IsValid)
            {
                course.Code = course.Code.ToUpper();
                _context.Update(course);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{course.Code} has been successfully updated.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", course.DepartmentId);
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Registrar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Find the course and include any existing relationships
            var course = await _context.Courses
                .Include(c => c.CourseRegistrations)
                .Include(c => c.CourseAssignments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            // DATA INTEGRITY CHECK: Do not allow deletion if the course is in use
            bool hasRegistrations = course.CourseRegistrations != null && course.CourseRegistrations.Any();
            bool hasAssignments = course.CourseAssignments != null && course.CourseAssignments.Any();

            if (hasRegistrations || hasAssignments)
            {
                TempData["ErrorMessage"] = $"Cannot delete {course.Code}. There are students registered for this course or lecturers assigned to teach it. Please drop them first.";
                return RedirectToAction(nameof(Index));
            }

            // If it's safe, delete it
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{course.Code} has been permanently deleted from the curriculum.";
            return RedirectToAction(nameof(Index));
        }
    }
}