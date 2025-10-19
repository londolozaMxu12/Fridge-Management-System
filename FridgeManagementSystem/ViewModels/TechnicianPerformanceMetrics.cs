using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianPerformanceMetrics
    {
        public int TotalFaults { get; set; }
        public int CompletedFaults { get; set; }
        public int InProgressFaults { get; set; }
        public int ScheduledFaults { get; set; }
        public int ReportedFaults { get; set; }
        public int CancelledFaults { get; set; }
        public int CriticalFaults { get; set; }
        public int HighPriorityFaults { get; set; }

        [DisplayFormat(DataFormatString = "{0:F1}")]
        public decimal AverageResolutionDays { get; set; }

        [DisplayFormat(DataFormatString = "{0:F1}%")]
        public decimal CompletionRate { get; set; }

        [DisplayFormat(DataFormatString = "{0:F1}%")]
        public decimal OnTimeCompletionRate { get; set; }

        public int TotalScheduledRepairs { get; set; }
        public int CompletedRepairs { get; set; }
    }
}