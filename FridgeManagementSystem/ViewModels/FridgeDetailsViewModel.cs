using FridgeManagementSystem.Models;

namespace FridgeManagementSystem.ViewModels
{
    public class FridgeDetailsViewModel
    {
        public Fridge Fridge { get; set; } = null!;
        public List<MaintenanceRecord> MaintenanceHistory { get; set; } = new List<MaintenanceRecord>();
        public List<Allocation> AllocationHistory { get; set; } = new List<Allocation>();
        public bool HasActiveAllocation { get; set; }
        public Allocation? CurrentAllocation { get; set; }
    }
}