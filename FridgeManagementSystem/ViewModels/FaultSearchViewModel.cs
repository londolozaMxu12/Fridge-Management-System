using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class FaultSearchViewModel
    {
        public string? Search { get; set; }

        public string? Priority { get; set; }

        public string? Status { get; set; }

        public string? Sort { get; set; }
    }
}
