using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public enum ScheduleStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled,
        Rescheduled
    }
    public class RepairSchedule
    {
        [Key]
        public int RepairScheduleId { get; set; }

        [Required]
        [Display(Name = "Scheduled Date")]
        public DateTime ScheduledDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        [Display(Name = "Schedule Status")]
        public ScheduleStatus Status { get; set; } = ScheduleStatus.Scheduled;

        [Display(Name = "Estimated Hours")]
        public decimal EstimatedHours { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }

        // Foreign Key
        [Required]
        public int FaultId { get; set; }
        public Fault Fault { get; set; }

        [Required]
        [Display(Name = "Fault Technician")]
        public int FaultTechnicianId { get; set; }
        public Employee FaultTechnician { get; set; }

        
    }
}