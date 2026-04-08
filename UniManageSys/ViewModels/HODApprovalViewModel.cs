using UniManageSys.Models;

namespace UniManageSys.ViewModels
{
    public class HODApprovalViewModel
    {
        public Department Department { get; set; } = null!;
        public List<CourseRegistration> PendingRegistrations { get; set; } = new List<CourseRegistration>();
    }
}