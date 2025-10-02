using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public enum FaultStatus
    {
        Pending,       // just reported and as a default
        InProgress,    // technician assigned
        Resolved      // repaired
              
    }

    public class Fault
    {
        [Key]
        public int FaultId { get; set; }

        [Required, StringLength(200)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Reported Date")]
        public DateTime ReportedDate { get; set; } = DateTime.Now;

        [Required]
        public FaultStatus Status { get; set; } = FaultStatus.Pending;

        // Foreign Keys
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int? FridgeId { get; set; }
        public Fridge? Fridge { get; set; }

        [Display(Name = "Fault Technician")]
        public int? EmployeeId { get; set; }
        public Employee FaultTechnician { get; set; }
        
        public RepairSchedule RepairSchedule { get; set; }
    }
}
