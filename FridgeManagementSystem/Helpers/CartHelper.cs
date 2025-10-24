using FridgeManagementSystem.ViewModels;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Helpers
{
    public class CartHelper
    {
        public static Dictionary<int, int> GetCartDictionary(HttpRequest request, HttpResponse response)
        {
            string cookieValue = request.Cookies["shopping_cart"] ?? "";

            try
            {
                var cart = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cookieValue));
                Console.WriteLine("[CartHelper] cart=" + cookieValue + " -> " + cart);
                var dictionary = JsonSerializer.Deserialize<Dictionary<int, int>>(cart);
                if (dictionary != null)
                {
                    return dictionary;
                }
            }
            catch (Exception)
            {
            }

            if (cookieValue.Length > 0)
            {
                // this cookie is not valid => delete it
                response.Cookies.Delete("shopping_cart");
            }

            return new Dictionary<int, int>();
        }

        public static int GetCartSize(HttpRequest request, HttpResponse response)
        {
            int cartSize = 0;
            var cartDictionary = GetCartDictionary(request, response);
            foreach (var keyValuePair in cartDictionary)
            {
                cartSize += keyValuePair.Value;
            }
            return cartSize;
        }

        public static List<CartItemViewModel> GetCartItems(HttpRequest request, HttpResponse response, FridgeManagementSystemContext context)
        {
            var cartItems = new List<CartItemViewModel>();
            var cartDictionary = GetCartDictionary(request, response);

            foreach (var pair in cartDictionary)
            {
                int fridgeId = pair.Key;
                int quantity = pair.Value;

                var fridge = context.Fridges
                    .Include(f => f.FridgeType)
                    .FirstOrDefault(f => f.FridgeId == fridgeId && f.Status == "Available");

                if (fridge == null) continue;

                var item = new CartItemViewModel
                {
                    FridgeId = fridgeId,
                    Quantity = quantity,
                    UnitPrice = fridge.Price,
                    Fridge = fridge,
                    FridgeName = fridge.FridgeType.Name,
                    FridgeBrand = fridge.FridgeType.Brand,
                    FridgeModel = fridge.FridgeType.Model,
                    ImageFileName = fridge.ImageFileName
                };

                cartItems.Add(item);
            }

            return cartItems;
        }

        public static decimal GetSubtotal(List<CartItemViewModel> cartItems)
        {
            decimal subtotal = 0;
            foreach (var item in cartItems)
            {
                subtotal += item.Quantity * item.UnitPrice;
            }
            return subtotal;
        }

        public static void SaveCartDictionary(Dictionary<int, int> cartDictionary, HttpResponse response)
        {
            string cartString = JsonSerializer.Serialize(cartDictionary);
            string cookieValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cartString));

            response.Cookies.Append("shopping_cart", cookieValue, new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
        }

        public static void AddToCart(int fridgeId, int quantity, HttpRequest request, HttpResponse response)
        {
            var cart = GetCartDictionary(request, response);

            if (cart.ContainsKey(fridgeId))
            {
                cart[fridgeId] += quantity;
            }
            else
            {
                cart[fridgeId] = quantity;
            }

            SaveCartDictionary(cart, response);
        }

        public static void RemoveFromCart(int fridgeId, HttpRequest request, HttpResponse response)
        {
            var cart = GetCartDictionary(request, response);

            if (cart.ContainsKey(fridgeId))
            {
                cart.Remove(fridgeId);
                SaveCartDictionary(cart, response);
            }
        }

        public static void UpdateCartQuantity(int fridgeId, int quantity, HttpRequest request, HttpResponse response)
        {
            var cart = GetCartDictionary(request, response);

            if (quantity <= 0)
            {
                RemoveFromCart(fridgeId, request, response);
            }
            else
            {
                cart[fridgeId] = quantity;
                SaveCartDictionary(cart, response);
            }
        }

        public static void ClearCart(HttpResponse response)
        {
            response.Cookies.Delete("shopping_cart");
        }

        // New method to transfer cart from cookies to database when user logs in
        public static async Task TransferCartToDatabase(string userId, HttpRequest request, HttpResponse response, FridgeManagementSystemContext context)
        {
            var cookieCart = GetCartDictionary(request, response);

            if (!cookieCart.Any()) return;

            var databaseCart = await context.ShoppingCart
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (databaseCart == null)
            {
                databaseCart = new ShoppingCart { UserId = userId };
                context.ShoppingCart.Add(databaseCart);
                await context.SaveChangesAsync();
            }

            foreach (var item in cookieCart)
            {
                var existingItem = databaseCart.Items.FirstOrDefault(i => i.FridgeId == item.Key);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Value;
                }
                else
                {
                    databaseCart.Items.Add(new CartItem
                    {
                        FridgeId = item.Key,
                        Quantity = item.Value,
                        AddedAt = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();

            // Clear the cookie cart after transfer
            ClearCart(response);
        }
    }
}