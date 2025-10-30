using System;
using System.Collections.Generic;

namespace FridgeManagementSystem.ViewModels
{
    public class CustomerAnalysisViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CustomerTypeFilter { get; set; }

        // Summary Statistics
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int InactiveCustomers { get; set; }
        public int PendingApproval { get; set; }

        // Trend Analysis
        public List<CustomerTrendData> MonthlyTrends { get; set; }
        public List<CustomerTypeDistribution> TypeDistribution { get; set; }
        public List<CustomerStatusHistory> StatusChanges { get; set; }

        // Detailed Lists
        public List<CustomerViewModel> RecentlyActivated { get; set; }
        public List<CustomerViewModel> RecentlyDeactivated { get; set; }
        public List<CustomerViewModel> LongTermInactive { get; set; }

        public CustomerAnalysisViewModel()
        {
            MonthlyTrends = new List<CustomerTrendData>();
            TypeDistribution = new List<CustomerTypeDistribution>();
            StatusChanges = new List<CustomerStatusHistory>();
            RecentlyActivated = new List<CustomerViewModel>();
            RecentlyDeactivated = new List<CustomerViewModel>();
            LongTermInactive = new List<CustomerViewModel>();
        }
    }

    public class CustomerTrendData
    {
        public string Period { get; set; }
        public int NewCustomers { get; set; }
        public int Activated { get; set; }
        public int Deactivated { get; set; }
        public int TotalActive { get; set; }
    }

    public class CustomerTypeDistribution
    {
        public string CustomerType { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }

    public class CustomerStatusHistory
    {
        public string CustomerName { get; set; }
        public string BusinessName { get; set; }
        public string Action { get; set; } // Activated, Deactivated
        public DateTime Date { get; set; }
        public string ChangedBy { get; set; }
    }
}
   
