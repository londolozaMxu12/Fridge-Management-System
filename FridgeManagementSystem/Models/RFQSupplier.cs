using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class RFQSupplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RFQId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Display(Name = "Sent Date")]
        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Responded")]
        public bool IsResponded { get; set; } = false;

        [Display(Name = "Response Date")]
        public DateTime? ResponseDate { get; set; }

        // Navigation properties
        public virtual RFQ RFQ { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
