using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class PurchaseRequest
    {
        [Key]
        public int PurchaseRequestId { get; set; }
        [Required, StringLength(50)]
        public string FridgeName { get; set; }
        [Required, StringLength(50)]
        public string FridgeCode { get; set; }
        [Required, StringLength(50)]
        public string FridgeType { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
