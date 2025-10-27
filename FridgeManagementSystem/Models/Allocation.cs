using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Allocation
    {
        [Key]
        public int AllocationId { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        [Required]
        [Display(Name = "Allocated By")]
        public string AllocatedById { get; set; } = "";
        public ApplicationUser AllocatedBy { get; set; } = null!;

        [Required]
        [Display(Name = "Fridge")]
        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; } = null!;

        //public DateTime? ServiceDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "Allocation Date")]
        public DateTime AllocationDate { get; set; } = DateTime.Now;

        public int? OrderId { get; set; }
        public Order? Order { get; set; } = null!;
    }
}