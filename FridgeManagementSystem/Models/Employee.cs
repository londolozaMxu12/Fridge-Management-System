using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [Display(Name = "Employee Number")]
        [StringLength(20)]
        public string EmployeeNo { get; set; } = "";

        [Required]
        [Display(Name = "Job Title")]
        [StringLength(100)]
        public string JobTitle { get; set; } = "";

        [Required]
        [Display(Name = "Employee Type")]
        public int EmployeeTypeId { get; set; }
        public EmployeeType EmployeeType { get; set; } = null!;

        [Required]
        [Display(Name = "Date Employed")]
        [DataType(DataType.Date)]
        public DateTime DateEmployed { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedById { get; set; } = "";
        public ApplicationUser CreatedBy { get; set; } = null!;

        public ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();
        public ICollection<PurchasingOrder> PurchasingOrders { get; set; } = new List<PurchasingOrder>();
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public ICollection<ScheduleMaintenance> ScheduledMaintenances { get; set; } = new List<ScheduleMaintenance>();
        public ICollection<Fault> ReportedFaults { get; set; } = new List<Fault>();
        public ICollection<RepairSchedule> FaultSchedules { get; set; } = new List<RepairSchedule>();
    }
}