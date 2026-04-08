using Microsoft.AspNetCore.Identity.UI.Services;

namespace UniManageSys.Services
{
    // This implements the interface that the Register page is looking for
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For now, we do absolutely nothing. It just "pretends" to succeed.
            // Later, you can drop real SMTP or SendGrid code in here!
            return Task.CompletedTask;
        }
    }
}