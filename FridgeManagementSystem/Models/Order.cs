using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using FridgeManagementSystem.Areas.Identity.Data;

namespace FridgeManagementSystem.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = "";
        public ApplicationUser Customer { get; set; } = null!;

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        [Precision(16, 2)]
        public decimal ShippingFee { get; set; }

        public string DeliveryAddress { get; set; } = "";
        public string PaymentMethod { get; set; } = "CashOnDelivery";
        public string PaymentStatus { get; set; } = "pending";
        public string PaymentDetails { get; set; } = "";
        public string OrderStatus { get; set; } = "Received";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? FridgeId { get; set; }
        public Fridge? Fridge { get; set; }
    }
}