using System;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class ScheduleMaintenance
    {
        [Key]
        public int scheduleMaintenanceId { get; set; }

        public string Description { get; set; }
        [Required]
        [Display(Name = "ScheduledDate")]
        public DateTime ScheduledDate { get; set; }

        public int MaintenanceTechId { get; set; }
        public MaintenanceTech MaintenanceTech { get; set; }
       
    }
}
