using System.ComponentModel.DataAnnotations;
using UniManageSys.Enums;

namespace UniManageSys.ViewModels
{
    public class LecturerOnboardViewModel
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
        [Display(Name = "Staff ID")]
        public string StaffId { get; set; } = string.Empty; // e.g., "CUSTAFF-001"

        [Required]
        [Display(Name = "Academic Rank")]
        public AcademicRank Rank { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
    }
}