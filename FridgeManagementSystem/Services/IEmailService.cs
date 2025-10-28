namespace FridgeManagementSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
        Task SendRFQToSupplier(Supplier supplier, RFQ rfq);
    }
}
