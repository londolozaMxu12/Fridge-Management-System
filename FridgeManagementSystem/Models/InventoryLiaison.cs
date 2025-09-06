using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class InventoryLiaison
    {
        [Key]
        public int InventoryLiaisonId { get; set; }

        [Required, StringLength(50)]
        public string FullName { get; set; }
        [Required, StringLength(100)]
        public string Email { get; set; }

        [Required, Phone]
        public string ContactNo { get; set; }
    }
}
