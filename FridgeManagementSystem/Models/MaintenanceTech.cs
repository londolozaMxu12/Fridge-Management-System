using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class MaintenanceTech
    {
        [Key]
        public int MaintenanceTechId { get; set; }

        [Required(ErrorMessage = "Please Enter Full Name"), StringLength(50)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Please Enter Email Address"), StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please Enter Contact Number"), Phone]
        [Display(Name = "Contact Number")]
        public string ContactNo { get; set; }


        //public ScheduleMaintenance ScheduleMaintenance { get; set; }
    }
}
