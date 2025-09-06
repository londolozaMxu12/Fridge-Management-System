using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required, StringLength(50)]
        public string FullName { get; set; }
        [Required, StringLength(50)]
        public string Email { get; set; }

        [Required, Phone]
        public string ContactNo { get; set; }
        public ICollection<PurchasingOrder> PurchasingOrders { get; set; }
    }
}
