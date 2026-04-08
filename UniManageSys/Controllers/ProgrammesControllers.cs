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
    public class ProgrammesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProgrammesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var query = _context.Programmes.Include(p => p.Department).AsQueryable();

            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                query = query.Where(p => p.DepartmentId == hodProfile!.DepartmentId);
            }

            var programmes = await query.OrderBy(p => p.Name).ToListAsync();
            return View(programmes);
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
        public async Task<IActionResult> Create(Programme programme)
        {
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                if (programme.DepartmentId != hodProfile!.DepartmentId)
                {
                    TempData["ErrorMessage"] = "You can only create programmes for your own department.";
                    return RedirectToAction(nameof(Index));
                }
            }

            ModelState.Remove("Department");
            ModelState.Remove("Students");
            if (ModelState.IsValid)
            {
                _context.Add(programme);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{programme.Name} created successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", programme.DepartmentId);
            return View(programme);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var programme = await _context.Programmes.FindAsync(id);
            if (programme == null) return NotFound();

            var deptQuery = _context.Departments.AsQueryable();
            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);

                if (programme.DepartmentId != hodProfile!.DepartmentId)
                {
                    TempData["ErrorMessage"] = "Access Denied.";
                    return RedirectToAction(nameof(Index));
                }
                deptQuery = deptQuery.Where(d => d.Id == hodProfile.DepartmentId);
            }

            ViewBag.Departments = new SelectList(await deptQuery.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", programme.DepartmentId);
            return View(programme);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Programme programme)
        {
            if (id != programme.Id) return NotFound();

            if (User.IsInRole("HOD"))
            {
                var user = await _userManager.GetUserAsync(User);
                var hodProfile = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == user!.Id);
                if (programme.DepartmentId != hodProfile!.DepartmentId) return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                _context.Update(programme);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Programme updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", programme.DepartmentId);
            return View(programme);
        }
    }
}