namespace FridgeManagementSystem.ViewModels
{
    public class EmployeeViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string PostalCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public List<string> Roles { get; set; }
    }
}
