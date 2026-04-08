using UniManageSys.Models;

namespace UniManageSys.ViewModels
{
    public class PersonalTimetableViewModel
    {
        public Semester ActiveSemester { get; set; } = null!;

        // A dictionary to group events by Monday, Tuesday, etc.
        public Dictionary<DayOfWeek, List<TimetableEvent>> WeeklySchedule { get; set; } = new();
    }
}