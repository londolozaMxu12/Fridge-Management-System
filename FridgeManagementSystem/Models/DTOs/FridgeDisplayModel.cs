namespace FridgeManagementSystem.Models.DTOs
{
    public class FridgeDisplayModel
    {
        public IEnumerable<Fridge> Fridges { get; set; }
        public IEnumerable<FridgeType> FridgeTypes { get; set; }
        public string searchTerm { get; set; }
        public int FridgeTypeId { get; set; }
    }
}
