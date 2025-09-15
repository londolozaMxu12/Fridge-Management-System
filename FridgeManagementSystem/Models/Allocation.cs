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


        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; }


        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public AllocationStatus Status { get; set; }
    }
}
