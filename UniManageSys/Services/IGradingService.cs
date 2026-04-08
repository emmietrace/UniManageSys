using System.Threading.Tasks;

namespace UniManageSys.Services
{
    public interface IGradingService
    {
        Task<GradingResult> ProcessScoreAsync(int registrationId, int lecturerId, decimal caScore, decimal examScore);
    }
}