using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;
using UniManageSys.ViewModels;

[Authorize(Roles = "Student")]
public class ResultsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ResultsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> MyTranscript()
    {
        var user = await _userManager.GetUserAsync(User);
        var student = await _context.Students
            .Include(s => s.Programme)
            .FirstOrDefaultAsync(s => s.UserId == user!.Id);

        var history = await _context.CourseRegistrations
            .Include(cr => cr.Course)
            .Include(cr => cr.Semester)
            .Include(cr => cr.Result)
            .Where(cr => cr.StudentId == student!.Id && cr.Status == UniManageSys.Enums.RegistrationStatus.Approved)
            .ToListAsync();

        return View(new TranscriptViewModel { Student = student!, AcademicHistory = history });
    }
}