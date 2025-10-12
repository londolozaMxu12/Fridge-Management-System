namespace FridgeManagementSystem.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddItem(int fridgeId, int quantity);
        Task<int> RemoveItem(int fridgeId);
        Task<ShoppingCart> GetUserCart();
        Task<ShoppingCart> GetCart(string userId);
        Task<int> GetCartItemCount(string userId = "");
    }
}
