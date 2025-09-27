using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class EmployeeEditViewModel
    {
        public string Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string ContactNo { get; set; }
        
        public string Address { get; set; }
        
        public string City { get; set; }
        
        public string Suburb { get; set; }

        public bool IsActive { get; set; }
        
        public string PostalCode { get; set; }
        [Required]
        public string EmployeeNo { get; set; }
        [Required]
        public int EmployeeTypeId { get; set; }
        [Required]
        public string JobTitle { get; set; }
        [Required]
        //public string SelectedRole { get; set; }
        [Display(Name = "Employee Role")]
        public string EmployeeRole { get; set; }
        [Required]
        public DateTime DateEmployed { get; set; }
    }
}
