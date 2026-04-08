using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace UniManageSys.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = String.Empty;

        [Required]
        [StringLength(10)]
        [Display(Name = "Department Code")]
        public string Code { get; set; } = String.Empty;

        [Required(ErrorMessage = "Please select a Faculty.")]
        public int FacultyId { get; set; }

        // FK Link to faculty
        [ForeignKey("FacultyId")]
        public Faculty? Faculty { get; set; }

        // Navigation ppty: One Dept. has many Programmes
        public ICollection<Programme> Programmes { get; set; } = new List<Programme>();

        // The ID of the lecturer who is currently the HOD
        public int? HODId { get; set; }

        [ForeignKey("HODId")]
        public Lecturer? HOD { get; set; }
    }
}
