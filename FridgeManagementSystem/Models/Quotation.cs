using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Quotation
    {
        [Key]
        public int QuotationId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required, StringLength(200)]
        public string Details { get; set; }

        // Foreign Key
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
