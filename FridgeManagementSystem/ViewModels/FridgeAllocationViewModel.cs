using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class FridgeAllocationViewModel
    {
        public int FridgeId { get; set; }
        public string FridgeSerialNumber { get; set; }
        public string FridgeModel { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Required]
        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime ServiceDate { get; set; } = DateTime.Today.AddDays(7);

        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}
