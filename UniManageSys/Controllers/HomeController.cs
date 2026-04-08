using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UniManageSys.Models;

namespace UniManageSys.Controllers
{
    [AllowAnonymous] // Anyone can visit the public website
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // If the user is already logged in, you can automatically redirect them to their dashboard!
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Student")) return RedirectToAction("StudentDashboard", "Dashboard");
                if (User.IsInRole("Lecturer") || User.IsInRole("HOD")) return RedirectToAction("LecturerDashboard", "Dashboard");
                if (User.IsInRole("SuperAdmin") || User.IsInRole("Registrar")) return RedirectToAction("AdminDashboard", "Dashboard");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}