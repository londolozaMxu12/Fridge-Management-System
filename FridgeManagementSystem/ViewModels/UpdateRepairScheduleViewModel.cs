using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class UpdateRepairScheduleViewModel
    {
        public int RepairScheduleId { get; set; }

        [Required]
        [Display(Name = "Scheduled Date")]
        public DateTime ScheduledDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        [Display(Name = "Schedule Status")]
        public ScheduleStatus Status { get; set; }

        [Required]
        [Display(Name = "Estimated Hours")]
        [Range(0.5, double.MaxValue, ErrorMessage = "Estimated hours must be at least 0.5")]
        public decimal EstimatedHours { get; set; }
    }
}
