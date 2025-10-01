using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class FridgeType
    {
        [Key]
        public int FridgeTypeId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(100)]
        public string Brand { get; set; }

        [Required]
        public string Model { get; set; }
        public ICollection<Fridge> Fridges { get; set; }
    }
}
