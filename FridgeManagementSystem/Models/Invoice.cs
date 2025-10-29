using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(16,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(16,2)")]
        public decimal ShippingFee { get; set; }

        [Required]
        [Column(TypeName = "decimal(16,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Paid, Overdue, Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Company information (could be moved to settings)
        public string CompanyName { get; set; } = "NM Design Hub";
        public string CompanyAddress { get; set; } = "123 Main Street, Gqeberha, Summerstrand, 6001";
        public string CompanyPhone { get; set; } = "(+27) 722 783 987";
        public string CompanyEmail { get; set; } = "nmdesignhub@gmail.com";

        // PDF file storage (optional - you can store the PDF in database or file system)
        public byte[] PdfData { get; set; }

        [StringLength(50)]
        public string PdfFileName { get; set; }

        // Navigation property for invoice items (if you want detailed line items)
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }

    
}


