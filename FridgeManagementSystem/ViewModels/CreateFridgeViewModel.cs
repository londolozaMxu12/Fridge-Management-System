using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class CreateFridgeViewModel
    {
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Serial Number")]
        [StringLength(100, ErrorMessage = "Serial Number cannot exceed 100 characters")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "Acquisition Date is required")]
        [Display(Name = "Acquisition Date")]
        [DataType(DataType.Date)]
        public DateTime AcquisitionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Available";

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

        [Required(ErrorMessage = "Please upload an image")]
        [Display(Name = "Fridge Image")]
        public IFormFile ImageFileName { get; set; }
    }
}