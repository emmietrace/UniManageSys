namespace UniManageSys.ViewModels
{
    public class AttendanceReportViewModel
    {
        public int CourseAssignmentId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int TotalClassesHeld { get; set; }

        public List<StudentAttendanceSummary> StudentSummaries { get; set; } = new();
    }

    public class StudentAttendanceSummary
    {
        public string MatricNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int ClassesAttended { get; set; }

        public double AttendancePercentage { get; set; }
        public bool IsEligibleForExam => AttendancePercentage >= 75.0; // The Magic 75% Rule!
    }
}