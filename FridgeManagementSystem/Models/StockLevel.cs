using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class StockLevel
    {
        [Key]
        public int StockLevelId { get; set; }
        [Required, StringLength(50)]
        public string FridgeName { get; set; }
        [Required, StringLength(50)]
        public string FridgeType { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public DateTime LastUpdated { get; set; }
        
    }
}
