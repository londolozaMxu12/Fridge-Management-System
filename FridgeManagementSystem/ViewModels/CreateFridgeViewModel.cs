using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class CreateFridgeViewModel
    {
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Fridge Type is required")]
        [Display(Name = "Fridge Type")]
        public int FridgeTypeId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Serial Number is required")]
        [StringLength(50, ErrorMessage = "Serial Number cannot exceed 50 characters")]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Acquisition Date is required")]
        [Display(Name = "Acquisition Date")]
        [DataType(DataType.Date)]
        public DateTime AcquisitionDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Available";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        [Display(Name = "Category")]
        public string Category { get; set; } = "Refrigerator";

        [Display(Name = "Next Service Date")]
        [DataType(DataType.Date)]
        public DateTime? NextServiceDate { get; set; }

        [Required(ErrorMessage = "Image is required")]
        [Display(Name = "Fridge Image")]
        public IFormFile ImageFile { get; set; } = null!;
    }
}