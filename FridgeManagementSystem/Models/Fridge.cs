using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Fridge
    {
        [Key]
        public int FridgeId { get; set; }

        [Required, StringLength(50)]
        public string SerialNumber { get; set; }

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        public string Model { get; set; }

        // Foreign Key
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public ICollection<Fault> Faults { get; set; }
    }
}