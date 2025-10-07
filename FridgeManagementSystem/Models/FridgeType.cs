using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Models
{
    public class FridgeType
    {
        [Key]
        public int FridgeTypeId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        [StringLength(100)]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Model is required")]
        [StringLength(100)]
        public string Model { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public ICollection<Fridge> Fridges { get; set; } = new List<Fridge>();
        // Display property for dropdown
        [NotMapped]
        public string DisplayName => $"{Brand} - {Name} - {Model}";
    }
}
