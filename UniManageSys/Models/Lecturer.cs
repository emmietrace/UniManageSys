using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniManageSys.Enums;

namespace UniManageSys.Models
{
    public class Lecturer
    {
        [Key]
        public int Id { get; set; }

        // --- 1-TO-1 LINK TO APPLICATION USER ---
        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        // --- EMPLOYMENT DETAILS ---
        [Required]
        [StringLength(20)]
        [Display(Name = "Staff ID")]
        public string StaffId { get; set; } = string.Empty; // e.g., "CUSTAFF-001"

        [Required]
        public AcademicRank Rank { get; set; }

        // --- LINK TO ACADEMIC STRUCTURE ---
        // A lecturer belongs to a specific department
        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        // Navigation Property: The courses they are assigned to teach
        public ICollection<CourseAssignment> CourseAssignments { get; set; } = new List<CourseAssignment>();
    }
}