using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class City
    {
        [Key]
        public int CityId { get; set; }

        [Required(ErrorMessage = "Please Enter City Name"), StringLength(50)]
        public string Name { get; set; }

        // Foreign Key
        public int ProvinceId { get; set; }
        public Province Province { get; set; }
        public ICollection<Suburb> Suburbs { get; set; }
    }
}
