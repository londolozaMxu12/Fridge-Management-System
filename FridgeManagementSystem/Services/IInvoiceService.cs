using FridgeManagementSystem.Models;
using System.Threading.Tasks;

namespace FridgeManagementSystem.Services
{
    public interface IInvoiceService
    {
        Task<Invoice> GenerateInvoiceAsync(int orderId);
        Task<Invoice> GetInvoiceByOrderIdAsync(int orderId);
        Task<byte[]> GenerateInvoicePdfAsync(int invoiceId);
        Task<byte[]> GetInvoicePdfAsync(int invoiceId);
    }
}