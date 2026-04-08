using System.ComponentModel.DataAnnotations;
namespace UniManageSys.Models
{
    public class AcademicYear
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Academic Session")]
        public string Name { get; set; } = String.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        // Only one year can be active at a time, so we can use this to determine the current year
        public bool IsActive { get; set; } = false;

        // Nav. ppty: One academic year has many semesters
        public ICollection<Semester> Semesters { get; set; } = new List<Semester>();
    }
}
