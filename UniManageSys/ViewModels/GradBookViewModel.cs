using UniManageSys.Models;

namespace UniManageSys.ViewModels
{
    public class GradebookViewModel
    {
        // This holds the specific class details (Course Name, Semester, etc.)
        public CourseAssignment Assignment { get; set; } = null!;

        // This holds the roster of students who are APPROVED to take this class
        public List<CourseRegistration> RegisteredStudents { get; set; } = new List<CourseRegistration>();
    }
}