namespace UniManageSys.Services
{
    // A clean wrapper object to pass success/failure messages back to the UI
    public class RegistrationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}