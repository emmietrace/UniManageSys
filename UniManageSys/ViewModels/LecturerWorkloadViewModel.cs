using Microsoft.AspNetCore.Mvc.Rendering;
using UniManageSys.Models;

namespace UniManageSys.ViewModels
{
    public class LecturerWorkloadViewModel
    {
        public Lecturer LecturerProfile { get; set; } = null!;
        public Semester ActiveSemester { get; set; } = null!;

        // The input fields for the assignment form
        public int SelectedCourseId { get; set; }
        public bool IsPrimaryLecturer { get; set; } = true;

        // The dropdown list of courses from their department
        public SelectList? AvailableCourses { get; set; }
    }
}