using System.ComponentModel.DataAnnotations;
using UniManageSys.Enums;

namespace UniManageSys.ViewModels
{
    public class TakeAttendanceViewModel
    {
        [Required]
        public int CourseAssignmentId { get; set; }

        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime SessionDate { get; set; } = DateTime.Today;

        [StringLength(200)]
        public string? TopicTaught { get; set; }

        // This list will hold the rows of students on our form
        public List<StudentAttendanceItem> Students { get; set; } = new List<StudentAttendanceItem>();
    }

    public class StudentAttendanceItem
    {
        public int StudentId { get; set; }
        public string MatricNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        // We default everyone to 'Present' to save the lecturer time
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        public string? Remarks { get; set; }
    }
}