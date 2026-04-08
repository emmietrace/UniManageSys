using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UniManageSys.Models
{
    public class Programme
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Progrmme Name")]
        public string Name { get; set; } = String.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Programme Code")]
        public string Code { get; set; } = String.Empty;

        [Required]
        [Display(Name = "Duration (Years)")]
        public int DurationInYears { get; set; } = 4;

        // FK Link to department
        [Required]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }
    }
}
