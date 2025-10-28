using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class PurchaseDashboardViewModel
    {
        [Display(Name = "Pending Requests")]
        public int PendingRequestsCount { get; set; }

        [Display(Name = "Active RFQs")]
        public int ActiveRFQsCount { get; set; }

        [Display(Name = "Open Purchase Orders")]
        public int OpenPOCount { get; set; }

        [Display(Name = "Pending Deliveries")]
        public int PendingDeliveriesCount { get; set; }

        [Display(Name = "Total Spend This Month")]
        [DataType(DataType.Currency)]
        public decimal MonthlySpend { get; set; }

        public List<PurchaseRequest> RecentRequests { get; set; } = new List<PurchaseRequest>();
        public List<PurchasingOrder> RecentPurchaseOrders { get; set; } = new List<PurchasingOrder>();
        public List<RFQ> UrgentRFQs { get; set; } = new List<RFQ>();

        // Statistics for charts
        public Dictionary<string, int> RequestsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> SpendingByCategory { get; set; } = new Dictionary<string, decimal>();
    }
}
