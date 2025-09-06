using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class PurchasingManager
    {
        [Key]
        public int PurchasingManagerId { get; set; }

        [Required, StringLength(50)]
        public string FullName { get; set; }
        [Required, StringLength(50)]
        public string Email { get; set; }

        [Required, Phone]
        public string ContactNo { get; set; }
        public ICollection<PurchaseRequest> PurchaseRequests { get; set; }
        public ICollection<PurchasingOrder> PurchasingOrders { get; set; }
    }
}
