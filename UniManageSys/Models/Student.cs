using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniManageSys.Enums;
namespace UniManageSys.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        // --- Link to App. User 1 to 1 ---
        [Required]
        public string UserId { get; set; } = String.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        // Academic Details
        [StringLength(20)]
        [Display(Name = "Matriculation Number")]
        public string? MatriculationNumber { get; set; }

        [Required]
        [Display(Name = "Enrollment Year")]
        public int EnrollmentYear { get; set; }

        [Required]
        [Display(Name = "Current Level")]
        public int CurrentLevel { get; set; } = 100;

        [Required]
        public StudentStatus Status = StudentStatus.Admitted;

        // --- Link TO Academic Structure
        [Required]
        public int ProgrammeId { get; set; }

        [ForeignKey("ProgrammeId")]
        public Programme? Programme { get; set; }

    }
}
