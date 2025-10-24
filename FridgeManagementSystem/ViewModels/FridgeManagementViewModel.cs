using FridgeManagementSystem.Models;

namespace FridgeManagementSystem.ViewModels
{
    public class FridgeManagementViewModel
    {
        public List<Fridge> Fridges { get; set; } = new List<Fridge>();

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalCount { get; set; }

        // Sorting
        public string SortBy { get; set; } = "AcquisitionDate";
        public string SortOrder { get; set; } = "desc";

        // Filtering
        public string SearchString { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = string.Empty;
        public string BrandFilter { get; set; } = string.Empty;
        public string SupplierFilter { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Computed properties for pagination
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public int FirstItemIndex => (PageNumber - 1) * PageSize + 1;
        public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalCount);
    }
}