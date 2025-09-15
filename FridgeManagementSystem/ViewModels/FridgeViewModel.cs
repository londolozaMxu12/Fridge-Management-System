using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class FridgeViewModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Required, StringLength(100)]
        public string Brand { get; set; }
        [Required]
        public Decimal Price { get; set; }
        [Required]
        public string Description { get; set; }
        
        public IFormFile? ImageFile { get; set; }
        [Required, StringLength(50)]
        public string SerialNumber { get; set; }

        [Required]
        public string Model { get; set; }
    }
}
