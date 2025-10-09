using FridgeManagementSystem.Areas.Identity.Data;

namespace FridgeManagementSystem.ViewModels
{
    public class EmployeeManagementViewModel
    {
        public IEnumerable<ApplicationUser> Employees { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalCount { get; set; }
        public string SortBy { get; set; } = "EmployeeNo";
        public string SortOrder { get; set; } = "asc";
        public string SearchString { get; set; }
        public string EmployeeTypeFilter { get; set; }
        public string StatusFilter { get; set; } = "active";
    }
}
