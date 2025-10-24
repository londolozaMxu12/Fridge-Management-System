namespace FridgeManagementSystem.ViewModels
{
    public class FridgeSummaryViewModel
    {
        public int TotalFridges { get; set; }
        public int AvailableFridges { get; set; }
        public int AllocatedFridges { get; set; }
        public int MaintenanceFridges { get; set; }
        public int InServiceFridges { get; set; }
        public decimal TotalInventoryValue { get; set; }

        // Recent activity
        public int FridgesAddedThisMonth { get; set; }
        public int FridgesAllocatedThisMonth { get; set; }
    }
}