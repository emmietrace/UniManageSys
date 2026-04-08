using UniManageSys.Models;

namespace UniManageSys.ViewModels
{
    public class TranscriptViewModel
    {
        public Student Student { get; set; } = null!;
        public List<CourseRegistration> AcademicHistory { get; set; } = new List<CourseRegistration>();

        // Cumulative GPA Calculation
        public decimal CGPA => AcademicHistory.Any(h => h.Result != null)
            ? AcademicHistory.Where(h => h.Result != null).Sum(h => h.Result!.GradePoint * h.Course!.CreditUnits)
              / AcademicHistory.Where(h => h.Result != null).Sum(h => h.Course!.CreditUnits)
            : 0;
    }
}