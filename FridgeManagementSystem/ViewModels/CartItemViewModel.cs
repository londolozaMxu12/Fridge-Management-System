namespace FridgeManagementSystem.ViewModels
{
    public class CartItemViewModel
    {
        public int FridgeId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public Fridge? Fridge { get; set; }
        public string FridgeName { get; set; } = "";
        public string FridgeBrand { get; set; } = "";
        public string FridgeModel { get; set; } = "";
        public string ImageFileName { get; set; } = "";

        /// Calculated total price for this cart item
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
