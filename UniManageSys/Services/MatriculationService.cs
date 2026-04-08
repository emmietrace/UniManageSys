using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
namespace UniManageSys.Services
{
    public class MatriculationService : IMatriculationService
    {
        private readonly ApplicationDbContext _context;
        public MatriculationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateMatricNumberAsync(int enrollmentYear, int programmesId)
        {
            string yearSuffix = enrollmentYear.ToString().Substring(2, 2);

            var programme = await _context.Programmes
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == programmesId);

            if (programme == null || programme.Department == null)
                throw new Exception("Invalid Programme or Department");

            string deptCode = programme.Department.Code.ToUpper();

            string prefix = $"{yearSuffix}{deptCode}";

            // 4. Find the last student enrolled in this specific batch to get the highest sequence number
            var lastStudent = await _context.Students
                .Where(s => s.MatriculationNumber != null && s.MatriculationNumber.StartsWith(prefix))
                .OrderByDescending(s => s.MatriculationNumber)
                .FirstOrDefaultAsync();

            int nxtSequenceNumber = 1;

            if (lastStudent != null && lastStudent.MatriculationNumber != null)
            {
                // Extract the last 3 digits and add 1
                string lastSequenceStr = lastStudent.MatriculationNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequenceStr, out int lastSequence))
                {
                    nxtSequenceNumber = lastSequence + 1;
                }
            }
            // 
            return $"{prefix}{nxtSequenceNumber:D3}";
        }
    }
}
