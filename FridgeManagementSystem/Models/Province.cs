using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Province
    {
        [Key]
        public int ProvinceId { get; set; }

        [Required(ErrorMessage = "Please Enter Province Name"), StringLength(50)]
        [Display(Name = "Province Name")]
        public string ProvinceName { get; set; }

        public ICollection<City> Cities { get; set; }
    }
}
