using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UniManageSys.Models;

namespace UniManageSys.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // The Sign Out button we made triggers a POST request, which lands here
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Kills the user's secure cookie
            await _signInManager.SignOutAsync();

            // Send them back to the Home page
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }
    }
}