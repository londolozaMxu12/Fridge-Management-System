using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Suburb
    {
        [Key]
        public int SuburbId { get; set; }
        [Required(ErrorMessage = "Please Enter Suburb Name"), StringLength(50)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please Enter Postal Code")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        // Foreign Key
        public int CityId { get; set; }
        public City City { get; set; }

        public ICollection<Customer> Customers { get; set; }
    }
}
