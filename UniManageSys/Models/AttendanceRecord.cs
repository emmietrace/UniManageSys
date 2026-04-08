using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UniManageSys.Enums;

namespace UniManageSys.Models
{
    public class AttendanceRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClassSessionId { get; set; }
        [ForeignKey("ClassSessionId")]
        public ClassSession? ClassSession { get; set; }

        [Required]
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [StringLength(100)]
        public string? Remarks { get; set; } // e.g., "Arrived 30 mins late"
    }
}