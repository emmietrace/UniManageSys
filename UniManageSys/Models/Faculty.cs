using System.ComponentModel.DataAnnotations;
namespace UniManageSys.Models
{
    public class Faculty
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Faculty Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Faculty Code")]
        public string Code { get; set; } = string.Empty;

        public ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
