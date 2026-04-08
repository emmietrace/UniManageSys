using System.Threading.Tasks;

namespace UniManageSys.Services
{
    public interface ICourseRegistrationService
    {
        Task<RegistrationResult> RegisterCourseAsync(int studentId, int courseId, int semesterId);
        Task<RegistrationResult> SubmitRegistrationAsync(int studentId, int semesterId);
    }
}