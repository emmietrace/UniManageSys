using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UniManageSys.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Course Code")]
        public string Code { get; set; } = String.Empty;

        [Required]
        [StringLength (150)]
        [Display(Name = "Course Title")]
        public string Title { get; set; } = String.Empty;

        [Required]
        [Range(1,6)]
        [Display(Name = "Credit Units")]
        public int CreditUnits { get; set; }

        [Required]
        public int Level { get; set; }

        // 1 = First Semester, 2 = Second Semester
        [Required]
        public int SemesterOffered { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        // The list of prerequisites required BEFORE taking this course
        public ICollection<CoursePrerequisite> Prerequisites { get; set; } = new List<CoursePrerequisite>();
        public ICollection<CourseRegistration>? CourseRegistrations { get; set; }
        public ICollection<CourseAssignment>? CourseAssignments { get; set; }

    }
}
