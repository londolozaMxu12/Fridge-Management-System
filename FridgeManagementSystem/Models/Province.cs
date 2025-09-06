using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Province
    {
        [Key]
        public int ProvinceId { get; set; }

        [Required, StringLength(50)]
        public string ProvinceName { get; set; }

        public ICollection<City> Cities { get; set; }
    }
}
