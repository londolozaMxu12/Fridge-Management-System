using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class MaintenanceRecord
    {
        [Key]
        public int MaintenanceRecordId { get; set; }

        [Required, StringLength(50)]
        public string Description { get; set; }

        [Required]
        public DateTime date { get; set; }

        // Foreign Key
        public int MaintenanceTechId { get; set; }
        public MaintenanceTech MaintenanceTech { get; set; }
        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; }

        public ICollection<FaultReport> FaultReports { get; set; }
    }
}
