using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManageSys.Models
{
    public class CourseAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LecturerId { get; set; }

        [ForeignKey("LecturerId")]
        public Lecturer? Lecturer { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        [Required]
        public int SemesterId { get; set; }

        [ForeignKey("SemesterId")]
        public Semester? Semester { get; set; }

        // Is this the main lecturer, or just an assisting co-lecturer?
        public bool IsPrimaryLecturer { get; set; } = true;
    }
}