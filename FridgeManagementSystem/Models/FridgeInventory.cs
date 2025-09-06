using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class FridgeInventory
    {
        [Key]
        public int FridgeIventoryId { get; set; }

        [Required, StringLength(50)]
        public string FridgeName { get; set; }

        [Required, StringLength(50)]
        public string FridgeType { get; set; }

        [Required, StringLength(100)]
        public string ScrappedFridges { get; set; }

        [Required]
        public DateTime DeliveryDate { get; set; }

        [Required]
        public DateTime DeliveryTime { get; set; }
    }
}
