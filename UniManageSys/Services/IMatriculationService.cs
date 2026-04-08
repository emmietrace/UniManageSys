using System.Threading.Tasks;
namespace UniManageSys.Services
{
    public interface IMatriculationService
    {
        Task<string> GenerateMatricNumberAsync(int enrollmentYear, int programmesId);
    }
}
