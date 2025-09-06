using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class FaultReport
    {
        [Key]
        public int FaultReportId { get; set; }
        [Required]
        public DateTime ReportDate { get; set; }
        [Required, StringLength(200)]
        public string Description { get; set; }
        [Required, StringLength(50)]
        public string Status { get; set; }
        // Foreign Key
        public int MaintenanceRecordId { get; set; }
        public MaintenanceRecord MaintenanceRecord { get; set; }
    }
}
