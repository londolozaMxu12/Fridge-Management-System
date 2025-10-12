using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public enum FaultStatus
    {
        Reported = 1,
        Scheduled = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5
    }

    public enum FaultPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public class Fault
    {
        [Key]
        public int FaultId { get; set; }

        [Required]
        [Display(Name = "Fault Title")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(200)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Reported Date")]
        public DateTime ReportedDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Priority")]
        public FaultPriority Priority { get; set; } = FaultPriority.Medium;

        [Required]
        [Display(Name = "Status")]
        public FaultStatus Status { get; set; } = FaultStatus.Reported;

        [Display(Name = "Scheduled Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ScheduledDate { get; set; }

        [Display(Name = "Fault Technician")]
        public int? FaultTechnicianId { get; set; }
        public Employee FaultTechnician { get; set; }

        [Display(Name = "Resolution Notes")]
        [StringLength(1000)]
        public string? ResolutionNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [Display(Name = "Reported By")]
        public int ReportedById { get; set; }
        public Customer ReportedBy { get; set; }

        public int? FridgeId { get; set; }
        public Fridge? Fridge { get; set; }

        public ICollection<RepairSchedule> RepairSchedules { get; set; }
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; }

    }

}
