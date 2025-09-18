using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FridgeManagementSystem.Areas.Identity.Pages.Account.Manage
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // For testing only – no real email is sent
            Console.WriteLine($"Email to {email} | Subject: {subject}");
            return Task.CompletedTask;
        }
    }
}
