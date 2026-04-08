using UniManageSys.Models;

namespace UniManageSys.Services
{
    public interface ITimetableService
    {
        Task<SchedulingResult> AddEventAsync(TimetableEvent newEvent);
    }
}