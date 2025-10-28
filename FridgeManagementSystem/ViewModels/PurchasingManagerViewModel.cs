using Microsoft.AspNetCore.Mvc.Rendering;

namespace FridgeManagementSystem.ViewModels
{
    public class PurchasingManagerViewModel
    {

        public int PendingPurchaseRequests { get; set; }
        public int OpenRFQs { get; set; }
        public int ActivePurchaseOrders { get; set; }
        public int PendingApprovals { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int RejectedCount { get; set; }

        public List<RecentPurchaseRequest> RecentPurchaseRequests { get; set; } = new();
        public List<RecentRFQ> RecentRFQs { get; set; } = new();
        public List<RecentPurchaseOrder> RecentPurchaseOrders { get; set; } = new();
        public List<PendingAction> PendingActions { get; set; } = new();
        public List<MonthlyStat> MonthlyStats { get; set; } = new();
    }

    public class RecentPurchaseRequest
    {
        public int Id { get; set; }
        public string PRNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "secondary";
        public bool IsUrgent { get; set; }
    }

    public class RecentRFQ
    {
        public int Id { get; set; }
        public string RFQNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int SupplierCount { get; set; }
        public bool IsDueSoon { get; set; }
    }

    public class RecentPurchaseOrder
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "secondary";
    }

    public class PendingAction
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string PriorityColor { get; set; } = "warning";
        public string DueIn { get; set; } = string.Empty;
    }

    public class MonthlyStat
    {
        public string Month { get; set; } = string.Empty;
        public int PurchaseOrdersCount { get; set; }
        public int PurchaseRequestsCount { get; set; }
    }

    //PurchaseDashboardViewModel.cs
    //public class PurchaseDashboardViewModel
    //{
    //    public int PendingPurchaseRequests { get; set; }
    //    public int OpenRFQs { get; set; }
    //    public int ActivePurchaseOrders { get; set; }
    //    public int PendingApprovals { get; set; }
    //    public List<DashboardPurchaseRequest> RecentPurchaseRequests { get; set; } = new();
    //    public List<DashboardPendingAction> PendingActions { get; set; } = new();
    //}

    public class DashboardPurchaseRequest
    {
        public int Id { get; set; }
        public string PRNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "secondary";
        public DateTime CreatedDate { get; set; }
    }

    public class DashboardPendingAction
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string DueIn { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string PriorityColor { get; set; } = "warning";
    }

    // PurchaseRequestListViewModel.cs
    public class PurchaseRequestListViewModel
    {
        public int Id { get; set; }
        public string PRNumber { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "secondary";
        public DateTime CreatedDate { get; set; }
        public bool CanApprove { get; set; }
    }

    // RFQListViewModel.cs
    public class RFQListViewModel
    {
        public int Id { get; set; }
        public string RFQNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int SupplierCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "secondary";
        public DateTime CreatedDate { get; set; }
        public bool CanEvaluate { get; set; }
    }

    // QuotationListViewModel.cs
    public class QuotationListViewModel
    {
        public int Id { get; set; }
        public string QuoteNumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string RFQNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime ValidityDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "secondary";
        public DateTime SubmittedDate { get; set; }
    }

    // PurchaseOrderListViewModel.cs
    public class PurchaseOrderListViewModel
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "secondary";
        public DateTime DeliveryDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool CanEdit { get; set; }
        public bool IsUrgent { get; set; }
    }

    // SupplierListViewModel.cs
    public class SupplierListViewModel
    {
        public int Id { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
    // PurchaseOrderViewModel.cs
    public class PurchaseOrderViewModel
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "secondary";
        public DateTime DeliveryDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool CanEdit { get; set; }
        public bool IsUrgent { get; set; }
    }

    // PurchaseOrderDetailViewModel.cs
    public class PurchaseOrderDetailViewModel
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "secondary";
        public DateTime DeliveryDate { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public List<POItemViewModel> Items { get; set; } = new();
        public bool ShowActions { get; set; }
        public bool CanApprove { get; set; }
        public bool CanCancel { get; set; }
        public bool CanEdit { get; set; }
    }

    public class POItemViewModel
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    // CreatePurchaseOrderViewModel.cs
    public class CreatePurchaseOrderViewModel
    {
        public int SupplierId { get; set; }
        public List<SupplierSelectItem> Suppliers { get; set; } = new();
        public DateTime DeliveryDate { get; set; } = DateTime.Now.AddDays(7);
        public string PaymentTerms { get; set; } = "Net 30";
        public string ShippingMethod { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<CreatePOItemViewModel> Items { get; set; } = new() { new CreatePOItemViewModel() };
    }

    public class SupplierSelectItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreatePOItemViewModel
    {
        public string Description { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }
}
