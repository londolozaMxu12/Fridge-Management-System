using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class CreateRFQViewModel
    {
        public int PurchaseRequestId { get; set; }

        [Required]
        [Display(Name = "Item Description")]
        public string ItemDescription { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "Specifications")]
        public string Specifications { get; set; }

        [Required]
        [Display(Name = "RFQ Status")]
        public RFQStatus RFQStatus { get; set; } 
        public List<SelectListItem> StatusOptions { get; set; } = new();

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Deadline for Quotes")]
        public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required(ErrorMessage = "Please select at least one supplier")]
        [Display(Name = "Select Suppliers")]
        public List<int> SelectedSupplierIds { get; set; } = new List<int>();

        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }
}
