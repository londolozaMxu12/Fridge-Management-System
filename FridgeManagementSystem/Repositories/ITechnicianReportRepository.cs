using FridgeManagementSystem.ViewModels;

namespace FridgeManagementSystem.Repositories
{
    public interface ITechnicianReportRepository
    {
        Task<List<Employee>> GetActiveTechniciansAsync();
        Task<TechnicianReportViewModel> GenerateTechnicianReportAsync(TechnicianReportViewModel parameters);
        Task<byte[]> GenerateTechnicianPdfReportAsync(TechnicianReportViewModel report);
        Task<byte[]> GenerateTechnicianExcelReportAsync(TechnicianReportViewModel report);
    }
}
