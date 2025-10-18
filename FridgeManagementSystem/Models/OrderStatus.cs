using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class OrderStatus
    {
        [Key]
        public int OrderStatusId { get; set; }

        [Required, MaxLength(20)]
        public string? OrderStatusName { get; set; }
        //[Required]
        //public int StatusId { get; set; }
    }
}
