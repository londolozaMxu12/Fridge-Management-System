namespace FridgeManagementSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        // User Statistics
        public int TotalUsers { get; set; }
        public int PendingApprovals { get; set; }
        public int ActiveCustomers { get; set; }
        public int TotalEmployees { get; set; }

        // Fridge Statistics
        public int TotalFridges { get; set; }
        public int AvailableFridges { get; set; }
        public int AllocatedFridges { get; set; }
        public int UnderRepairFridges { get; set; }

        // Supplier Statistics
        public int TotalSuppliers { get; set; }
        public int ActiveSuppliers { get; set; }

        // Distributions
        public Dictionary<string, int> EmployeeTypeDistribution { get; set; } = new();
        public Dictionary<string, int> FridgeStatusDistribution { get; set; } = new();

        // Recent Activities
        public List<RecentActivityViewModel> RecentUsers { get; set; } = new();
        public List<RecentActivityViewModel> RecentFridges { get; set; } = new();
    }
}
