using UniManageSys.Models;

namespace UniManageSys.ViewModels
{
    public class LecturerClassesViewModel
    {
        public Lecturer LecturerProfile { get; set; } = null!;
        public Semester ActiveSemester { get; set; } = null!;

        // The list of courses they are teaching this semester
        public List<CourseAssignment> AssignedCourses { get; set; } = new List<CourseAssignment>();
    }
}