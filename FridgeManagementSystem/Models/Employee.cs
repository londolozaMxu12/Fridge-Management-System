using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Employee Number")]
        [StringLength(20)]
        public string EmployeeNo { get; set; } // Auto-generated: EMP001, EMP002, etc.

        [Required]
        [Display(Name = "Job Title")]
        [StringLength(100)]
        public string JobTitle { get; set; }

        [Required]
        [Display(Name = "Employee Type")]
        public int EmployeeTypeId { get; set; }
        public EmployeeType EmployeeType { get; set; }

        [Required]
        [Display(Name = "Date Employed")]
        [DataType(DataType.Date)]
        public DateTime DateEmployed { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }

        public ICollection<PurchaseRequest> PurchaseRequests { get; set; }
        public ICollection<PurchasingOrder> PurchasingOrders { get; set; }
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; }
        public ICollection<ScheduleMaintenance> ScheduledMaintenances { get; set; }
        
        //For Fault Scheduling
        public ICollection<Fault> ReportedFaults { get; set; }
        public ICollection<Fault> AssignedFaults { get; set; }
        public ICollection<RepairSchedule> AssignedFaultSchedules { get; set; }
        public ICollection<FaultAssignment> FaultAssignments { get; set; }

    }
}
