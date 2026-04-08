namespace UniManageSys.Enums
{
    public enum RegistrationStatus
    {
        Draft = 0,       // Student is selecting courses but hasn't submitted
        Pending = 1,     // Submitted, waiting for HOD/Adviser approval
        Approved = 2,    // Officially registered
        Rejected = 3,    // HOD denied it (e.g., missing prerequisites)
        Dropped = 4      // Student dropped the course before the deadline
    }
}