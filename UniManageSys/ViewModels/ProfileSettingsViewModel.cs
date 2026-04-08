using System.ComponentModel.DataAnnotations;

namespace UniManageSys.ViewModels
{
    public class ProfileSettingsViewModel
    {
        // --- PROFILE SECTION ---
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        // Read-only info for the UI
        public string Email { get; set; } = string.Empty;
        public string? MatricNumber { get; set; }
        public string? DepartmentCode { get; set; } // Read-only info for Lecturers
        public string? Role { get; set; }

        // --- PASSWORD SECTION ---
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}