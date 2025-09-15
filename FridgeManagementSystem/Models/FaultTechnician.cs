using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class FaultTechnician
    {
        [Key]
        public int FaultTechnicianId { get; set; }

        [Required(ErrorMessage = "Please Enter Full Name"), StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please Enter Email Address"), StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please Enter Contact Number"), Phone]
        [Display(Name = "Contact Number")]
        public string ContactNo { get; set; }

        public ICollection<Fault> Faults { get; set; }
    }
}