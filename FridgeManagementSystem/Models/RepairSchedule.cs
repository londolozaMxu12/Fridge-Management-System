using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class RepairSchedule
    {
        [Key]
        public int RepairScheduleId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }

        // Foreign Key
        public int FaultId { get; set; }
        public Fault Fault { get; set; }
    }
}