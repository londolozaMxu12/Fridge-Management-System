using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string? FullName { get; set; } = "";
        [PersonalData]
        [Display(Name = "Contact Number")]
        public string? ContactNo { get; set; } = "";
        //public string DOB { get; set; }
        public string? Address { get; set; } = "";
        public string? City { get; set; } = "";
        public string? Suburb { get; set; } = "";
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public string ApprovalStatus { get; set; } = "Pending"; // Pending, Approved, Rejected

        public string? ApprovedById { get; set; }
        public ApplicationUser? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation properties
        //public ICollection<Customer>  Customers { get; set; }
        public Customer Customers { get; set; }
        //public ICollection<Employee> Employees { get; set; }
        public Employee Employees { get; set; }
        //public ICollection<Supplier> Suppliers { get; set; }
        public Supplier Suppliers { get; set; }

        public ShoppingCart ShoppingCart { get; set; }
    }
}
