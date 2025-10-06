using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        public ICollection<Fridge> Fridges { get; set; }
        // Display property for dropdown
        [NotMapped]
        public string DisplayName => $"{Brand} - {Name} - {Model}";
    }
}
