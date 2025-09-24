using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Supplier Type")]
        public string SupplierType { get; set; } // Fridge, Parts, Other

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public ICollection<PurchasingOrder> PurchasingOrders { get; set; }
        public ICollection<Fridge> Fridges { get; set; }
    }
}
