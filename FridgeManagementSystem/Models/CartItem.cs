using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; } = null!;

        [Required]
        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; } = null!;

        [Required]
        public int Quantity { get; set; } = 1;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}