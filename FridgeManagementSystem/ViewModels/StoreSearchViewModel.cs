namespace FridgeManagementSystem.ViewModels
{
    public class StoreSearchViewModel
    {
        public string? Brand { get; set; }
        public string? Category { get; set; }
        public string Sort { get; set; } = "newest";
        public string? Search { get; set; }

    }
}
