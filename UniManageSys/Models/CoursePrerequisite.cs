using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace UniManageSys.Models
{
    public class CoursePrerequisite
    {
        // The course students want to take
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        // Course they must have passed first
        public int PrerequisiteId { get; set; }

        [ForeignKey("PrerequisiteId")]
        public Course? Prerequisite { get; set; }

    }
}
