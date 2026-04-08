using System.Threading.Tasks;

namespace UniManageSys.Services
{
    public interface ICourseAssignmentService
    {
        // For adding a course to a lecturer's workload
        Task<AssignmentResult> AssignCourseAsync(int lecturerId, int courseId, int semesterId, bool isPrimary);

        // For removing a course from their workload if a mistake was made
        Task<AssignmentResult> RemoveAssignmentAsync(int assignmentId);
    }
}