using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class EmployeeType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
