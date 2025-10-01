using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class ShoppingCart
    {
        [Key]
        public int ShoppingCartId { get; set; }

        public string UserId { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
