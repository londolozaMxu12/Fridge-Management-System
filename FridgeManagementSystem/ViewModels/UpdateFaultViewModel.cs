using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class UpdateFaultViewModel
    {
        public int FaultId { get; set; }
        public string Title { get; set; }

        [Required(ErrorMessage = "Resolution notes are required")]
        [Display(Name = "Resolution Notes")]
        [StringLength(1000)]
        public string ResolutionNotes { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Fault Status")]
        public FaultStatus Status { get; set; }

        [Display(Name = "Mark as Completed")]
        public bool MarkCompleted { get; set; }
    }
}
