using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;

namespace UniManageSys.Controllers
{
    [Authorize(Roles = "SuperAdmin,Registrar,HOD")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            // Include HOD info so we can see who is running the department
            var departments = await _context.Departments
                .Include(d => d.HOD)
                    .ThenInclude(h => h!.User)
                .OrderBy(d => d.Name)
                .ToListAsync();
            return View(departments);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            // Fetch faculties for the dropdown
            ViewBag.Faculties = new SelectList(_context.Faculties.OrderBy(f => f.Name), "Id", "Name");
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            // IMPORTANT: Tell ModelState to ignore navigation properties
            ModelState.Remove("Faculty");
            ModelState.Remove("Programmes");
            ModelState.Remove("HOD");

            if (ModelState.IsValid)
            {
                if (await _context.Departments.AnyAsync(d => d.Code == department.Code))
                {
                    ModelState.AddModelError("Code", "A department with this code already exists.");
                    ViewBag.Faculties = new SelectList(_context.Faculties, "Id", "Name", department.FacultyId);
                    return View(department);
                }

                _context.Add(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{department.Name} Department created successfully.";
                return RedirectToAction(nameof(Index));
            }

            // If we fail, reload the dropdown
            ViewBag.Faculties = new SelectList(_context.Faculties, "Id", "Name", department.FacultyId);
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();
            ViewBag.Faculties = new SelectList(await _context.Faculties.OrderBy(f => f.Name).ToListAsync(), "Id", "Name", department.FacultyId);
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.Id) return NotFound();
            ModelState.Remove("Faculty");
            ModelState.Remove("Programmes");
            ModelState.Remove("HOD");

            if (ModelState.IsValid)
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Department updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Faculties = new SelectList(await _context.Faculties.OrderBy(f => f.Name).ToListAsync(), "Id", "Name", department.FacultyId);
            return View(department);
        }
    }
}