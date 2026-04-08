using System.ComponentModel.DataAnnotations;

namespace UniManageSys.Models
{
    public class Venue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "CST Lecture Theatre 1"

        [Required]
        public int Capacity { get; set; } // Helps us check if a class is too big for the room

        public bool IsLab { get; set; } = false; // Is this a laboratory or standard class?
    }
}