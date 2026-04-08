using UniManageSys.Models;

namespace UniManageSys.ViewModels
{
    // --- 1. THE UPGRADED STUDENT DASHBOARD ---
    public class StudentDashboardViewModel
    {
        // Your Existing Properties (Registration Engine)
        public Student StudentProfile { get; set; } = null!;
        public Semester CurrentSemester { get; set; } = null!;
        public List<Course> AvailableCourses { get; set; } = new List<Course>();
        public List<CourseRegistration> CurrentRegistrations { get; set; } = new List<CourseRegistration>();
        public int TotalRegisteredCredits => CurrentRegistrations
            .Where(cr => cr.Status != Enums.RegistrationStatus.Dropped)
            .Sum(cr => cr.Course?.CreditUnits ?? 0);
        public int MaxCredits { get; set; } = 24;

        // NEW Properties (Timetable Engine)
        public List<TimetableEvent> TodaysClasses { get; set; } = new List<TimetableEvent>();
    }

    // --- 2. THE LECTURER DASHBOARD ---
    public class LecturerDashboardViewModel
    {
        public Lecturer LecturerProfile { get; set; } = null!;
        public Semester CurrentSemester { get; set; } = null!;

        public int TotalStudents { get; set; }
        public int PendingAttendance { get; set; } // Optional: For a future feature
        // Courses they are assigned to teach this semester
        public List<CourseAssignment> AssignedCourses { get; set; } = new List<CourseAssignment>();

        // Their specific teaching schedule for today
        public List<TimetableEvent> TodaysLectures { get; set; } = new List<TimetableEvent>();
    }

    // --- 3. THE ADMIN/HOD DASHBOARD ---
    public class AdminDashboardViewModel
    {
        public int PendingApprovalsCount { get; set; }
        public string ActiveSemesterName { get; set; } = string.Empty;
        public IEnumerable<Student> RecentStudents { get; set; } = new List<Student>();
        public Semester? ActiveSemester { get; set; }

        // High-level University Stats
        public int TotalStudents { get; set; }
        public int TotalLecturers { get; set; }
        public int TotalCourses { get; set; }

        // Actionable Work items
        public int PendingRegistrations { get; set; }
    }
}