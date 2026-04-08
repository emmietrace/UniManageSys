using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniManageSys.Models
{
    public class Semester
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = String.Empty;

        // FK linking back to the Academic Year
        [Required]
        public int AcademicYearId { get; set; }

        [ForeignKey("AcademicYearId")]
        public AcademicYear? AcademicYear { get; set; } // Navigation property to the parent Academic Year

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Only one semester can be active at a time within an academic year
        public bool IsActive { get; set; } = false;
    }
}
