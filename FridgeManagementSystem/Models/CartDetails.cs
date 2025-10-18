using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Models
{
    public class CartDetails
    {
        [Key]
        public int CartDetailsId { get; set; }

        [Required]
        [Display(Name = "ShoppingCart")]
        public int? ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; }

        [Required]
        [Display(Name = "Fridge")]
        public int? FridgeId { get; set; }
        [Required, Precision(16, 2)]
        public Decimal UnitPrice { get; set; }
        public Fridge Fridge { get; set; }

        public int Quantity { get; set; }

    }
}
