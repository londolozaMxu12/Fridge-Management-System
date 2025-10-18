using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly FridgeManagementSystemContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(FridgeManagementSystemContext db, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<int> AddItem(int fridgeId, int quantity)
        {
            string userId = GetUserId();
            using var transaction = _db.Database.BeginTransaction();
            try
            {

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCart.Add(cart);
                }
                _db.SaveChanges();
                //Cart details
                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.ShoppingCartId && a.FridgeId == fridgeId);
                if (cartItem is not null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    var fridge = _db.Fridges.Find(fridgeId);
                    cartItem = new CartDetails
                    {
                        FridgeId = fridgeId,
                        ShoppingCartId = cart.ShoppingCartId,
                        Quantity = quantity,
                        UnitPrice = fridge.Price
                    };
                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {

            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<int> RemoveItem(int fridgeId)
        {
            string userId = GetUserId();
            //using var transaction = _db.Database.BeginTransaction();
            try
            {

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    //return false;
                    throw new Exception("Invalid Cart");
                }
                _db.SaveChanges();
                //Cart details
                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.ShoppingCartId && a.FridgeId == fridgeId);
                if (cartItem == null)
                {
                    throw new Exception("No items in cart");
                    //return false;
                }
                else if (cartItem.Quantity == 1)
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = cartItem.Quantity - 1;
                }
                _db.SaveChanges();
                //transaction.Commit();
                //return true;
            }
            catch (Exception ex)
            {
                //return false;
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new Exception("Invalid UserId");
            var shoppingCart = await _db.ShoppingCart.Include(a => a.CartDetails).ThenInclude(a => a.Fridge).ThenInclude(a => a.FridgeType).Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;
        }

        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (!string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }
            var data = await (from cart in _db.ShoppingCart
                              join cartDetails in _db.CartDetails
                              on cart.ShoppingCartId equals cartDetails.ShoppingCartId
                              select new { cartDetails.CartDetailsId }
                              ).ToListAsync();
            return data.Count;
        }

        public async Task<bool> DoCheckout()
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in.");
                var cart = await GetUserCart();
                if (cart is null)
                    throw new Exception("Invalid Cart");
                var cartDetails = _db.CartDetails.Where(a=>a.ShoppingCartId==cart.ShoppingCartId).ToList();
                if (cartDetails.Count == 0)
                    throw new Exception("Cart is Empty");
                var order = new PurchasingOrder
                {
                    UserId = userId,
                    Date = DateTime.UtcNow,
                    OrderStatusId = 1, //pending
                };
                _db.PurchasingOrders.Add(order);
                _db.SaveChanges();
                foreach(var item in cartDetails)
                {
                    var orderDetails = new PurchasingOrderDetails
                    {
                        FridgeId = item.FridgeId.Value,
                        PurchasingOrderId = order.PurchasingOrderId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _db.PurchasingOrderDetails.Add(orderDetails);
                }
                _db.SaveChanges();

                //removing cart details
                _db.CartDetails.RemoveRange(cartDetails);
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}
