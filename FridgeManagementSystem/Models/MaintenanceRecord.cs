using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Models
{
    public class MaintenanceRecord
    {
        [Key]
        public int MaintenanceRecordId { get; set; }

        [StringLength(50)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime ServiceDate { get; set; }

        [Display(Name = "Next Service Date")]
        [DataType(DataType.Date)]
        public DateTime? NextServiceDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required]
        [Display(Name = "Maintenance Technician")]
        public int MaintenanceTechnicianId { get; set; } 
        public Employee MaintenanceTechnician { get; set; }

        [Required]
        [Display(Name = "Fridge")]
        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; }

        public ICollection<FaultReport> FaultReports { get; set; }
    }
}
