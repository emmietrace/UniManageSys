using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniManageSys.Enums;

namespace UniManageSys.Models
{
    public class CourseRegistration
    {
        [Key]
        public int Id { get; set; }

        // --- THE STUDENT ---
        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        // --- THE COURSE ---
        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        // --- THE TIME CONTEXT ---
        [Required]
        public int SemesterId { get; set; }

        [ForeignKey("SemesterId")]
        public Semester? Semester { get; set; }

        // --- WORKFLOW & AUDIT ---
        [Required]
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Draft;

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedDate { get; set; }

        // Tracks who approved it (Could be the HOD's User ID)
        public string? ApprovedByUserId { get; set; }

        // Navigation property for the grading engine
        public StudentResult? Result { get; set; }
    }
}