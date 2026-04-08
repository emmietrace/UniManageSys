namespace UniManageSys.Services
{
    // A clean wrapper to pass assignment status back to the controller
    public class AssignmentResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}