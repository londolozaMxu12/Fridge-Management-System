namespace FridgeManagementSystem.ViewModels
{
    public class UserManagementViewModel
    {
        public IEnumerable<UserViewModel> Users { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalCount { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
        public string SearchString { get; set; }
        public string RoleFilter { get; set; }
        public string StatusFilter { get; set; }
        public string ApprovalFilter { get; set; }
    }
}
