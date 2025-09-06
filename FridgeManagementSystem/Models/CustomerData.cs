using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class CustomerData
    {
        [Key]
        public int CustomerDataId { get; set; }

        [Required, StringLength(50)]
        public string FullName { get; set; }

        [Required]
        public DateTime AllocationDate { get; set; }

        [Required]
        public DateTime AllocationTime { get; set; }
    }
}
