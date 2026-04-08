using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManageSys.Models
{
    public class ClassSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseAssignmentId { get; set; }
        [ForeignKey("CourseAssignmentId")]
        public CourseAssignment? CourseAssignment { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime SessionDate { get; set; } = DateTime.Today;

        [StringLength(200)]
        public string? TopicTaught { get; set; } // Optional: What was taught today?

        // Navigation property to see all records for this specific day
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}