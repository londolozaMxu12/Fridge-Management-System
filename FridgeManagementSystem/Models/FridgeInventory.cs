using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class FridgeInventory
    {
        [Key]
        public int FridgeIventoryId { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Fridge Name")]
        public string FridgeName { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Fridge Type")]
        public string FridgeType { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Scrapped Fridges")]
        public string ScrappedFridges { get; set; }

        [Required]
        [Display(Name = "Delivery Date")]
        public DateTime DeliveryDate { get; set; }

        [Required]
        [Display(Name = "Delivery Time")]
        public DateTime DeliveryTime { get; set; }
    }
}
