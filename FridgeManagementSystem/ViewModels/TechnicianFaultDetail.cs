using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianFaultDetail
    {
        public int FaultId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ReportedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public decimal ResolutionDays { get; set; }
        public string FridgeType { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public bool OnTimeCompletion { get; set; }
        public bool HasResolutionNotes => !string.IsNullOrEmpty(ResolutionNotes);
        public string ResolutionNotes { get; set; } = string.Empty;
    }
}