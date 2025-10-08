using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class EditFridgeViewModel
    {
        public int FridgeId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Serial Number is required")]
        [StringLength(100, ErrorMessage = "Serial Number cannot exceed 100 characters")]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "Acquisition Date is required")]
        [Display(Name = "Acquisition Date")]
        [DataType(DataType.Date)]
        public DateTime AcquisitionDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Fridge Type is required")]
        [Display(Name = "Fridge Type")]
        public int FridgeTypeId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Display(Name = "Next Service Date")]
        [DataType(DataType.Date)]
        public DateTime? NextServiceDate { get; set; }

        [Display(Name = "Last Service Date")]
        [DataType(DataType.Date)]
        public DateTime? ServiceDate { get; set; }

        [Display(Name = "Fridge Image")]
        public IFormFile? ImageFileName { get; set; }

        public string ExistingImagePath { get; set; }

        [Display(Name = "Selected Fridge Type")]
        public string SelectedFridgeTypeDisplay { get; set; }
    }
}