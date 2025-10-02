using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class PurchasingOrder
    {
        [Key]
        public int PurchasingOrderId { get; set; }
        [Required]
        public DateTime Date {  get; set; } = DateTime.UtcNow;
        [Required, StringLength(200)]
        public string Details { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public bool IsDeleted { get; set; }
        //[Required]
        //public OrderStatus Status { get; set; } = FaultStatus.Pending;


        // Foreign Key
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        //[Required]
        [Display(Name = "Purchasing Manager")]
        public int EmployeeId { get; set; }
        public Employee PurchasingManager { get; set; }

        public int OrderStatusId { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public ICollection<PurchasingOrderDetails> OrderDetails { get; set; }
    }
}
