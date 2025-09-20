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
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string PostalCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SelectedRole { get; set; }
    }
}
