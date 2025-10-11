namespace FridgeManagementSystem.ViewModels
{
    public class CustomerViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string BusinessName { get; set; }
        public string CustomerType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ApprovalStatus { get; set; }

        // Display properties
        public string StatusDisplay => IsActive ? "Active" : "Inactive";
        public string CreatedAtDisplay => CreatedAt.ToString("MM/dd/yyyy");
    }
}