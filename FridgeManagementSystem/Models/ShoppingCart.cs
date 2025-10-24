using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } = null!;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}