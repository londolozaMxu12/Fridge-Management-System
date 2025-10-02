using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public enum RequestStatus { Pending, Approved, Rejected }
    public class PurchaseRequest
    {
        [Key]
        public int PurchaseRequestId { get; set; }
        [Required, StringLength(50)]
        [Display(Name = "Fridge Name")]
        public string FridgeName { get; set; }
        [Required, StringLength(50)]
        public string FridgeCode { get; set; }
        [Display(Name = "Requested Date")]
        public DateTime DateRequested { get; set; }
        public DateTime RequestedBy { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Fridge Type")]
        public string FridgeType { get; set; }
        [Required]
        public int Quantity { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public string Notes { get; internal set; }

        [Display(Name = "Purchasing Manager")]
        public int EmployeeId { get; set; }
        public Employee PurchasingManager { get; set; }
    }
}
