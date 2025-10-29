namespace FridgeManagementSystem.ViewModels
{
    public class CustomerDetailsViewModel
    {
        public string Id { get; set; }
        public string BusinessName { get; set; }
        public string CustomerType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByFullName { get; set; }

        // User properties
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string PostalCode { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Counts for the view
        public int FridgeCount { get; set; }
        public int FaultCount { get; set; }
        
        public int AllocationCount { get; set; }
        public int ActiveOrdersCount { get; set; }
        public int TotalOrdersCount { get; set; }
        public int CompletedOrdersCount { get; set; }
        public int PendingOrdersCount { get; set; }
        public int ProcessingOrdersCount { get; set; }

        // Lists for detailed views
        public List<Fridge> Fridges { get; set; }
        public List<Order> Orders { get; set; }
    }
}
