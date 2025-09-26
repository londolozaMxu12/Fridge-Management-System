using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    //public enum FridgeStatus { Available, Allocated, UnderMaintenance, Scrapped }
    public class Fridge
    {
        [Key]
        public int FridgeId { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Required, StringLength(100)]
        public string Brand { get; set; }
        [Required]
        public string Model { get; set; }
        [Required, Precision(16, 2)]
        public Decimal Price { get; set; }
        [Required]
        public string Description { get; set; }
        [Required, MaxLength(255)]
        public string ImageFile { get; set; }
        [Required, StringLength(50)]
        public string SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? ScrapDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }=true;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        

        // Foreign Key
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int? SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public Allocation CurrentAllocation { get; set; }


    }
}