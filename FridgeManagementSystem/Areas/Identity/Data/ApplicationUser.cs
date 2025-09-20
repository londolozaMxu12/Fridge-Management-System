using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string? FullName { get; set; } = "";
        [PersonalData]
        public string? ContactNo { get; set; } = "";
        //public string DOB { get; set; }
        public string? Address { get; set; } = "";
        public string? City { get; set; } = "";
        public string? Suburb { get; set; } = "";
        public string? PostalCode { get; set; } = "";
        public bool? IsActive { get; set; } = true;
        public DateTime?CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
