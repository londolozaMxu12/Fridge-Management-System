using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public enum AllocationStatus { Active, Returned }
    public class Allocation
    {
        [Key]
        public int AllocationId { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int AllocatedById { get; set; }
        public ApplicationUser AllocatedBy { get; set; }

        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; }
        public bool IsActive { get; set; }=true;
      
        public DateTime AllocationDate { get; set; }=DateTime.Now;
        public DateTime? EndDate { get; set; }
        public AllocationStatus Status { get; set; }
    }
}
