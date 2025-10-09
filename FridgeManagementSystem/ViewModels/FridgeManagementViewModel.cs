using FridgeManagementSystem.Models;

namespace FridgeManagementSystem.ViewModels
{
    public class FridgeManagementViewModel
    {
        public IEnumerable<Fridge> Fridges { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalCount { get; set; }
        public string SortBy { get; set; } = "PurchaseDate";
        public string SortOrder { get; set; } = "desc";
        public string SearchString { get; set; }
        public string StatusFilter { get; set; }
        public string BrandFilter { get; set; }
        public string SupplierFilter { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
