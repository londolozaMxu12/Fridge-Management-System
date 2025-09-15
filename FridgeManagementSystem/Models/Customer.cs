using FridgeManagementSystem.Controllers;
using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Please Enter Full Name"), StringLength(50)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please Enter Address"), StringLength(200)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please Enter Contact Number"), Phone]
        [Display(Name = "Contact Number")]
        public string ContactNo { get; set; }

        // Foreign Key
        public int SuburbId { get; set; }
        public Suburb Suburb { get; set; }

        public ICollection<Fridge> Fridges { get; set; }
        public ICollection<Fault> Faults { get; set; }
        public ICollection<FridgeRequest> FridgeRequests { get; set; }
        public ICollection<Quotation> Quotations { get; set; }
        public ICollection<Allocation> Allocations { get; set; }
    }
}
