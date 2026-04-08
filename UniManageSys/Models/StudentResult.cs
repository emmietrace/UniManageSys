using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManageSys.Models
{
    public class StudentResult
    {
        [Key]
        public int Id { get; set; }

        // --- THE ACADEMIC CONTEXT ---
        // Links directly to the approved registration (Student + Course + Semester)
        [Required]
        public int CourseRegistrationId { get; set; }

        [ForeignKey("CourseRegistrationId")]
        public CourseRegistration? Registration { get; set; }

        // --- THE GRADER ---
        // Tracks exactly which lecturer uploaded this score
        [Required]
        public int GradedByLecturerId { get; set; }

        [ForeignKey("GradedByLecturerId")]
        public Lecturer? GradedBy { get; set; }

        // --- THE SCORES ---
        [Required]
        [Range(0, 30, ErrorMessage = "CA Score must be between 0 and 30")]
        public decimal ContinuousAssessment { get; set; } = 0;

        [Required]
        [Range(0, 70, ErrorMessage = "Exam Score must be between 0 and 70")]
        public decimal ExamScore { get; set; } = 0;

        [Required]
        [Range(0, 100)]
        public decimal TotalScore { get; set; } = 0;

        // --- COMPUTED OUTCOMES ---
        [StringLength(2)]
        public string Grade { get; set; } = string.Empty; // e.g., A, B, C, D, F

        public decimal GradePoint { get; set; } = 0; // e.g., 5.0 for A, 4.0 for B

        // --- WORKFLOW ---
        // Results are hidden from students until officially approved by the Senate/HOD
        public bool IsPublished { get; set; } = false;

        public DateTime DateUploaded { get; set; } = DateTime.UtcNow;
    }
}