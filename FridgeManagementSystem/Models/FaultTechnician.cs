using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class FaultTechnician
    {
        [Key]
        public int FaultTechnicianId { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [Required, Phone]
        public string ContactNo { get; set; }

        public ICollection<Fault> Faults { get; set; }
    }
}