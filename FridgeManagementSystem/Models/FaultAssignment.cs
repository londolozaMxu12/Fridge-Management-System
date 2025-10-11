using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Models
{
    public class FaultAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Repair Schedule")]
        public int RepairScheduleId { get; set; }
        public RepairSchedule RepairSchedule { get; set; }

        [Required]
        [Display(Name = "Technician")]
        public int TechnicianId { get; set; }

        [ForeignKey("TechnicianId")]
        public Employee Technician { get; set; }

        [Display(Name = "Assigned Hours")]
        public decimal AssignedHours { get; set; }

        [Display(Name = "Assignment Notes")]
        [StringLength(300)]
        public string Notes { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }

}
