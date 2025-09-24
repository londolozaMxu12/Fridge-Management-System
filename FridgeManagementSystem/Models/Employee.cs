using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Employee Number")]
        [StringLength(20)]
        public string EmployeeNo { get; set; } // Auto-generated: EMP001, EMP002, etc.

        [Required]
        [Display(Name = "Job Title")]
        [StringLength(100)]
        public string JobTitle { get; set; }

        [Required]
        [Display(Name = "Employee Type")]
        public int EmployeeTypeId { get; set; }
        public EmployeeType EmployeeType { get; set; }

        [Required]
        [Display(Name = "Date Employed")]
        [DataType(DataType.Date)]
        public DateTime DateEmployed { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
    }
}
