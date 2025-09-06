using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Suburb
    {
        [Key]
        public int SuburbId { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        [Required]
        public string PostalCode { get; set; }

        // Foreign Key
        public int CityId { get; set; }
        public City City { get; set; }

        public ICollection<Customer> Customers { get; set; }
    }
}
