using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Delivery address is required")]
        [Display(Name = "Delivery Address")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string DeliveryAddress { get; set; } = "";

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";
    }
}
