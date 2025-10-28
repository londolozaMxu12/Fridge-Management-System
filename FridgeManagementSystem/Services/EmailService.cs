using FridgeManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System;
using System.Net;
using System.Net.Mail;

namespace FridgeManagementSystem.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("youremail@gmail.com", "your_app_password"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("youremail@gmail.com"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
        public async Task SendRFQToSupplier(Supplier supplier, RFQ rfq)
        {
            string subject = $"New RFQ Request - {rfq.RFQNumber}";
            string body = $@"
                <h3>Hello {supplier.CompanyName},</h3>
                <p>You have received a new Request for Quotation (RFQ).</p>
                <p><strong>RFQ Details:</strong></p>
                <ul>
                    <li><strong>Name:</strong> {rfq.RFQNumber}</li>
                    <li><strong>Description:</strong> {rfq.ItemDescription}</li>
                    <li><strong>Status:</strong> {rfq.Status}</li>
                </ul>
                <p>Please log into the supplier portal to respond.</p>
                <p>Best regards,<br/>Fridge Management System</p>
            ";

            await SendEmailAsync(supplier.Email, subject, body);
        }
    }
}

