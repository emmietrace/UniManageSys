using System.ComponentModel.DataAnnotations;

namespace UniManageSys.ViewModels
{
    public class StudentEnrollmentViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "University Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Enrollment Year")]
        public int EnrollmentYear { get; set; } = DateTime.Now.Year;

        [Required]
        [Display(Name = "Programme of Study")]
        public int ProgrammeId { get; set; }

        // --- NEW PROPERTY ---
        [Required]
        [Display(Name = "Starting Level")]
        public int StartingLevel { get; set; } = 100;
    }
}