using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;

namespace UniManageSys.Services
{
    public class TimetableService : ITimetableService
    {
        private readonly ApplicationDbContext _context;

        public TimetableService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SchedulingResult> AddEventAsync(TimetableEvent newEvent)
        {
            // 1. Sanity Check
            if (newEvent.StartTime >= newEvent.EndTime)
                return new SchedulingResult { IsSuccess = false, Message = "End time must be after the start time." };

            // 2. VENUE CONFLICT CHECK
            // Is this room already booked today during this time?
            var venueConflict = await _context.TimetableEvents
                .Include(t => t.Course)
                .Where(t => t.SemesterId == newEvent.SemesterId
                         && t.VenueId == newEvent.VenueId
                         && t.Day == newEvent.Day)
                .FirstOrDefaultAsync(t => newEvent.StartTime < t.EndTime && newEvent.EndTime > t.StartTime);

            if (venueConflict != null)
            {
                return new SchedulingResult
                {
                    IsSuccess = false,
                    Message = $"Conflict: Venue is already booked for {venueConflict.Course?.Code} from {venueConflict.StartTime:hh\\:mm} to {venueConflict.EndTime:hh\\:mm}."
                };
            }

            // 3. LECTURER CONFLICT CHECK
            // Is this lecturer supposed to be somewhere else right now?
            var lecturerConflict = await _context.TimetableEvents
                .Include(t => t.Course)
                .Where(t => t.SemesterId == newEvent.SemesterId
                         && t.LecturerId == newEvent.LecturerId
                         && t.Day == newEvent.Day)
                .FirstOrDefaultAsync(t => newEvent.StartTime < t.EndTime && newEvent.EndTime > t.StartTime);

            if (lecturerConflict != null)
            {
                return new SchedulingResult
                {
                    IsSuccess = false,
                    Message = $"Conflict: The assigned lecturer is already teaching {lecturerConflict.Course?.Code} from {lecturerConflict.StartTime:hh\\:mm} to {lecturerConflict.EndTime:hh\\:mm}."
                };
            }

            // 4. Passed all checks! Save to database.
            _context.TimetableEvents.Add(newEvent);
            await _context.SaveChangesAsync();

            return new SchedulingResult
            {
                IsSuccess = true,
                Message = "Class scheduled successfully without conflicts."
            };
        }
    }
}