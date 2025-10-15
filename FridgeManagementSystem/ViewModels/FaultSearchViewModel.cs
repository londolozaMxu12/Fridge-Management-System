using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class FaultSearchViewModel
    {
        public IEnumerable<Fault> Faults { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 4;
        public string? Search { get; set; }

        public string? Priority { get; set; }

        public string? Status { get; set; }

        public string? Sort { get; set; }
    }
}
