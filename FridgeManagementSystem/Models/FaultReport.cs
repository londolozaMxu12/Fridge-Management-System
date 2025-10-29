using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Models
{
    public class FaultReport
    {
        [Key]
        public int FaultId { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Required]
        [Display(Name = "Fridge")]
        public int FridgeId { get; set; }

        [ForeignKey("FridgeId")]
        public Fridge Fridge { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Fault Description")]
        public string Description { get; set; }

        [Display(Name = "Date Reported")]
        public DateTime DateReported { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // Default status
    
        public int MaintenanceRecordId { get; set; }
        public MaintenanceRecord MaintenanceRecord { get; set; }
    }
}
