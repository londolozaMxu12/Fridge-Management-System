using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class CreateFaultViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Fault Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Fault Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority Level")]
        public FaultPriority Priority { get; set; } = FaultPriority.Medium;

        [Display(Name = "Select Fridge")]
        public int? FridgeId { get; set; }

        // Display properties
        public List<CustomerFridgeViewModel> CustomerFridges { get; set; } = new();
    }
}
