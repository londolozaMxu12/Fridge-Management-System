using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class MonthlyPerformance
    {
        public string Month { get; set; } = string.Empty;
        public int TotalFaults { get; set; }
        public int CompletedFaults { get; set; }

        [DisplayFormat(DataFormatString = "{0:F1}%")]
        public decimal CompletionRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:F1}")]
        public decimal AverageResolutionDays { get; set; }
    }
}