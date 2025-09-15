using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public enum Status
    {
        Pending,
        Approved,
        Declined
    }

    public class FridgeRequest
    {
        [Key]
        public int FridgeRequestId { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required, StringLength(200)]
        public string Reason { get; set; }

        [Required]
        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        public Status Status { get; set; } = Status.Pending;
    }

}