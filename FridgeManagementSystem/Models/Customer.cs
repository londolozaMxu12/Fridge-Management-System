using FridgeManagementSystem.Controllers;
using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required, StringLength(50)]
        public string FullName { get; set; }

        [Required, StringLength(200)]
        public string Address { get; set; }

        [Required, Phone]
        public string ContactNo { get; set; }

        // Foreign Key
        public int SuburbId { get; set; }
        public Suburb Suburb { get; set; }

        public ICollection<Fridge> Fridges { get; set; }
        public ICollection<Fault> Faults { get; set; }
        public ICollection<FridgeRequest> FridgeRequests { get; set; }
        public ICollection<Quotation> Quotations { get; set; }
    }
}
