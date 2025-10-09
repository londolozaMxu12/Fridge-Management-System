using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class AllocationViewModel
    {
        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Fridge is required")]
        [Display(Name = "Fridge")]
        public int FridgeId { get; set; }

        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime? ServiceDate { get; set; }

        // Display properties
        public string CustomerName { get; set; }
        public string CustomerBusiness { get; set; }
        public string FridgeDescription { get; set; }
        public string FridgeSerialNumber { get; set; }
        public decimal FridgePrice { get; set; }
    }

}
