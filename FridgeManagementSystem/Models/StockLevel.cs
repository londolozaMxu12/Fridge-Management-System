using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class StockLevel
    {
        [Key]
        public int StockLevelId { get; set; }
        [Required, StringLength(50)]
        [Display(Name = "Fridge Name")]
        public string FridgeName { get; set; }
        [Required, StringLength(50)]
        [Display(Name = "Fridge Type")]
        public string FridgeType { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; }

    }
}
