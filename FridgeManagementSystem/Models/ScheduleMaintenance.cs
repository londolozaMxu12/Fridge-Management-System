using FridgeManagementSystem.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FridgeManagementSystem.Models
{
    public class ScheduleMaintenance
    {
        [Key]
        public int scheduleMaintenanceId { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Display(Name = "ScheduledDate")]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledDate { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        [Required]
        [Display(Name = "Maintenance Technician")]
        public int MaintenanceTechnicianId { get; set; }
        [ForeignKey("MaintenanceTechnicianId")]
        public Employee MaintenanceTechnician { get; set; }

        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; }


    }
}
