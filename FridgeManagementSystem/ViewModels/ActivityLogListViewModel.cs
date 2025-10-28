using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class ActivityLogListViewModel
    {
        
            public List<ActivityLogViewModel> ActivityLogs { get; set; } = new List<ActivityLogViewModel>();

            [Display(Name = "Filter by Entity Type")]
            public string EntityTypeFilter { get; set; }

            [Display(Name = "Filter by Action")]
            public string ActionFilter { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "From Date")]
            public DateTime? FromDate { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "To Date")]
            public DateTime? ToDate { get; set; }

            // Filter options for dropdowns
            public List<string> EntityTypeOptions { get; set; } = new List<string>
        {
            "PurchaseRequest", "RFQ", "Quotation", "PurchaseOrder", "Supplier"
        };

            public List<string> ActionOptions { get; set; } = new List<string>
        {
            "Created", "Updated", "Deleted", "Approved", "Rejected", "Submitted", "Converted"
        };

            // Statistics
            public int TotalActivities { get; set; }
            public Dictionary<string, int> ActivitiesByType { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> ActivitiesByAction { get; set; } = new Dictionary<string, int>();
        }
    }

