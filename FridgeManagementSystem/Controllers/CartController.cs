using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Helpers;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    public class CartController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CartController> _logger;
        private readonly decimal _shippingFee;
        private readonly IOrderNotificationRepository _notification;

        public CartController(FridgeManagementSystemContext context,
                            UserManager<ApplicationUser> userManager,
                            IConfiguration configuration,
                            ILogger<CartController> logger, IOrderNotificationRepository notification)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _notification = notification;
            _shippingFee = configuration.GetValue<decimal>("CartSettings:ShippingFee", 0);
        }

        private string BuildDeliveryAddress(ApplicationUser user)
        {
            if (user == null) return string.Empty;

            var addressParts = new[]
            {
        user.Address,
        user.Suburb,
        user.City
    };

            // Filter out null/empty values and join with commas
            return string.Join(", ", addressParts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        public async Task<IActionResult> IndexAsync()
        {
            try
            {
                List<CartItemViewModel> cartItems;
                decimal subtotal;

                if (User.Identity.IsAuthenticated)
                {
                    // Get cart items from database for logged-in users
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = _context.ShoppingCart
                        .Include(c => c.Items)
                        .ThenInclude(i => i.Fridge)
                        .ThenInclude(f => f.FridgeType)
                        .FirstOrDefault(c => c.UserId == userId);

                    if (cart == null || !cart.Items.Any())
                    {
                        cartItems = new List<CartItemViewModel>();
                        subtotal = 0;
                    }
                    else
                    {
                        cartItems = cart.Items.Select(item => new CartItemViewModel
                        {
                            FridgeId = item.FridgeId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Fridge.Price,
                            Fridge = item.Fridge,
                            FridgeName = item.Fridge.FridgeType.Name,
                            FridgeBrand = item.Fridge.FridgeType.Brand,
                            FridgeModel = item.Fridge.FridgeType.Model,
                            ImageFileName = item.Fridge.ImageFileName
                        }).ToList();

                        subtotal = cartItems.Sum(item => item.Quantity * item.UnitPrice);
                    }
                }
                else
                {
                    // Get cart items from cookies for anonymous users
                    cartItems = CartHelper.GetCartItems(Request, Response, _context);
                    subtotal = CartHelper.GetSubtotal(cartItems);
                }

                ViewBag.CartItems = cartItems;
                ViewBag.ShippingFee = _shippingFee;
                ViewBag.Subtotal = subtotal;
                ViewBag.Total = subtotal + _shippingFee;

                // If user is logged in, pre-fill the form with their address
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var model = new CheckoutViewModel();

                    if (user != null)
                    {
                        model.DeliveryAddress = BuildDeliveryAddress(user);
                    }

                    return View(model);
                }

                return View(new CheckoutViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart page");
                ViewBag.ErrorMessage = "An error occurred while loading your cart";
                return View(new CheckoutViewModel());
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CheckoutViewModel model)
        {
            try
            {
                List<CartItemViewModel> cartItems;
                decimal subtotal;

                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = _context.ShoppingCart
                        .Include(c => c.Items)
                        .ThenInclude(i => i.Fridge)
                        .ThenInclude(f => f.FridgeType)
                        .FirstOrDefault(c => c.UserId == userId);

                    if (cart == null || !cart.Items.Any())
                    {
                        cartItems = new List<CartItemViewModel>();
                        subtotal = 0;
                    }
                    else
                    {
                        cartItems = cart.Items.Select(item => new CartItemViewModel
                        {
                            FridgeId = item.FridgeId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Fridge.Price,
                            Fridge = item.Fridge,
                            FridgeName = item.Fridge.FridgeType.Name,
                            FridgeBrand = item.Fridge.FridgeType.Brand,
                            FridgeModel = item.Fridge.FridgeType.Model,
                            ImageFileName = item.Fridge.ImageFileName
                        }).ToList();

                        subtotal = cartItems.Sum(item => item.Quantity * item.UnitPrice);
                    }
                }
                else
                {
                    cartItems = CartHelper.GetCartItems(Request, Response, _context);
                    subtotal = CartHelper.GetSubtotal(cartItems);
                }

                ViewBag.CartItems = cartItems;
                ViewBag.ShippingFee = _shippingFee;
                ViewBag.Subtotal = subtotal;
                ViewBag.Total = subtotal + _shippingFee;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check if shopping cart is empty or not
                if (!cartItems.Any())
                {
                    ViewBag.ErrorMessage = "Your cart is empty";
                    return View(model);
                }

                TempData["DeliveryAddress"] = model.DeliveryAddress;
                TempData["PaymentMethod"] = model.PaymentMethod;
                

                if (model.PaymentMethod == "CreditCard" || model.PaymentMethod == "PayPal")
                {
                    return RedirectToAction("Payment", "Orders");
                }

                return RedirectToAction("Confirm");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cart checkout");
                ViewBag.ErrorMessage = "An error occurred while processing your order";
                return View(model);
            }
        }

        [Authorize]
        public IActionResult Confirm()
        {
            try
            {
                List<CartItemViewModel> cartItems;
                decimal subtotal;
                int cartSize;

                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = _context.ShoppingCart
                        .Include(c => c.Items)
                        .ThenInclude(i => i.Fridge)
                        .ThenInclude(f => f.FridgeType)
                        .FirstOrDefault(c => c.UserId == userId);

                    if (cart == null || !cart.Items.Any())
                    {
                        TempData["ErrorMessage"] = "Your cart is empty";
                        return RedirectToAction("Index", "Cart");
                    }

                    cartItems = cart.Items.Select(item => new CartItemViewModel
                    {
                        FridgeId = item.FridgeId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Fridge.Price,
                        Fridge = item.Fridge,
                        FridgeName = item.Fridge.FridgeType.Name,
                        FridgeBrand = item.Fridge.FridgeType.Brand,
                        FridgeModel = item.Fridge.FridgeType.Model,
                        ImageFileName = item.Fridge.ImageFileName
                    }).ToList();

                    subtotal = cartItems.Sum(item => item.Quantity * item.UnitPrice);
                    cartSize = cart.Items.Sum(i => i.Quantity);
                }
                else
                {
                    cartItems = CartHelper.GetCartItems(Request, Response, _context);
                    subtotal = CartHelper.GetSubtotal(cartItems);
                    cartSize = cartItems.Sum(i => i.Quantity);
                }

                string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
                string paymentMethod = TempData["PaymentMethod"] as string ?? "";
                

                if (cartSize == 0 || deliveryAddress.Length == 0 || paymentMethod.Length == 0)
                {
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.DeliveryAddress = deliveryAddress;
                ViewBag.PaymentMethod = paymentMethod;
                
                ViewBag.Total = subtotal + _shippingFee;
                ViewBag.CartSize = cartSize;
                ViewBag.CartItems = cartItems;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading confirmation page");
                TempData["ErrorMessage"] = "An error occurred while loading confirmation";
                return RedirectToAction("Index", "Cart");
            }
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(string deliveryAddress, string paymentMethod)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction("Index", "Home");
                }

                List<CartItemViewModel> cartItems;
                List<CartItem> dbCartItems = new List<CartItem>();

                // Get cart items based on user authentication
                if (User.Identity.IsAuthenticated)
                {
                    var cart = await _context.ShoppingCart
                        .Include(c => c.Items)
                        .ThenInclude(i => i.Fridge)
                        .ThenInclude(f => f.FridgeType)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart == null || !cart.Items.Any())
                    {
                        TempData["ErrorMessage"] = "Your cart is empty";
                        return RedirectToAction("Index", "Cart");
                    }

                    cartItems = cart.Items.Select(item => new CartItemViewModel
                    {
                        FridgeId = item.FridgeId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Fridge.Price,
                        Fridge = item.Fridge,
                        FridgeName = item.Fridge.FridgeType.Name,
                        FridgeBrand = item.Fridge.FridgeType.Brand,
                        FridgeModel = item.Fridge.FridgeType.Model,
                        ImageFileName = item.Fridge.ImageFileName
                    }).ToList();

                    dbCartItems = cart.Items.ToList();
                }
                else
                {
                    cartItems = CartHelper.GetCartItems(Request, Response, _context);
                    if (!cartItems.Any())
                    {
                        TempData["ErrorMessage"] = "Your cart is empty";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                // Validate delivery address and payment method
                if (string.IsNullOrEmpty(deliveryAddress) || string.IsNullOrEmpty(paymentMethod))
                {
                    // Fallback to TempData if parameters are empty
                    deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
                    paymentMethod = TempData["PaymentMethod"] as string ?? "";
                }

                if (!cartItems.Any() || string.IsNullOrEmpty(deliveryAddress) || string.IsNullOrEmpty(paymentMethod))
                {
                    TempData["ErrorMessage"] = "Order information is missing. Please try again.";
                    return RedirectToAction("Index", "Cart");
                }

                // Validate fridge availability before creating order
                var unavailableFridges = new List<string>();
                foreach (var item in cartItems)
                {
                    var fridge = await _context.Fridges
                        .Include(f => f.FridgeType)
                        .FirstOrDefaultAsync(f => f.FridgeId == item.FridgeId && f.IsActive);

                    if (fridge == null || fridge.Status != "Available")
                    {
                        unavailableFridges.Add($"{item.FridgeName} (ID: {item.FridgeId})");
                    }
                }

                if (unavailableFridges.Any())
                {
                    TempData["ErrorMessage"] = $"The following items are no longer available: {string.Join(", ", unavailableFridges)}. Please update your cart.";
                    return RedirectToAction("Index", "Cart");
                }

                // Create the order
                var order = new Order
                {
                    CustomerId = userId,
                    DeliveryAddress = deliveryAddress,
                    PaymentMethod = paymentMethod,
                    PaymentStatus = "pending",
                    OrderStatus = "Received",
                    ShippingFee = _shippingFee,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<OrderItem>()
                };

                // Add order items and reserve fridges
                var reservedFridgeIds = new List<int>();
                foreach (var item in cartItems)
                {
                    // Add order item
                    order.Items.Add(new OrderItem
                    {
                        FridgeId = item.FridgeId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });

                    // Reserve the fridge (change status from Available to Reserved)
                    var fridge = await _context.Fridges
                        .Include(f => f.FridgeType)
                        .FirstOrDefaultAsync(f => f.FridgeId == item.FridgeId);

                    if (fridge != null && fridge.Status == "Available")
                    {
                        fridge.Status = "Reserved";
                        
                        // Find customer and associate with fridge for reservation
                        var customer = await _context.Customers
                            .FirstOrDefaultAsync(c => c.UserId == userId);
                        if (customer != null)
                        {
                            fridge.CustomerId = customer.Id;
                        }

                        reservedFridgeIds.Add(fridge.FridgeId);

                        _logger.LogInformation($"Reserved fridge {fridge.FridgeId} ({fridge.FridgeType.Name}) for order");
                    }
                    else
                    {
                        _logger.LogWarning($"Fridge {item.FridgeId} is not available for reservation. Status: {fridge?.Status}");
                    }
                }

                // Save the order to get the ID
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 🔔 NOTIFY ALL CUSTOMER LIAISONS ABOUT THE NEW ORDER
                await _notification.NotifyLiaisonsAboutNewOrder(order);

                // Clear the cart after successful order creation
                if (User.Identity.IsAuthenticated)
                {
                    // Clear database cart
                    var cart = await _context.ShoppingCart
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart != null)
                    {
                        _context.CartItems.RemoveRange(cart.Items);
                        _logger.LogInformation($"Cleared database cart for user {userId}");
                    }
                }
                else
                {
                    // Clear cookie cart
                    CartHelper.ClearCart(Response);
                    _logger.LogInformation("Cleared cookie cart");
                }

                // Final save to persist cart clearance
                await _context.SaveChangesAsync();

                // Log successful order creation
                _logger.LogInformation($"Order {order.Id} created successfully for user {userId}. {reservedFridgeIds.Count} fridges reserved.");

                // Set success message and return to confirmation view
                ViewBag.SuccessMessage = $"Order created successfully! Your order ID is: {order.Id}. " +
                                       $"We have reserved {reservedFridgeIds.Count} item(s) for you. " +
                                       $"You will be notified when your order is processed.";
                ViewBag.OrderId = order.Id;

                return View();
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while confirming order");
                ViewBag.ErrorMessage = "A database error occurred while creating your order. Please try again.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while confirming order");
                ViewBag.ErrorMessage = "An unexpected error occurred while creating your order. Please contact support if the problem persists.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int fridgeId, int quantity = 1)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    return await AddToDatabaseCart(fridgeId, quantity);
                }
                else
                {
                    return AddToCookieCart(fridgeId, quantity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding fridge to cart. FridgeId: {FridgeId}", fridgeId);
                return Json(new { success = false, message = "An error occurred while adding to cart" });
            }
        }

        private async Task<JsonResult> AddToDatabaseCart(int fridgeId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.ShoppingCart
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId };
                _context.ShoppingCart.Add(cart);
                await _context.SaveChangesAsync();
            }

            var fridge = await _context.Fridges
                .FirstOrDefaultAsync(f => f.FridgeId == fridgeId && f.Status == "Available" && f.IsActive);

            if (fridge == null)
            {
                return Json(new { success = false, message = "Fridge is no longer available" });
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.FridgeId == fridgeId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    FridgeId = fridgeId,
                    Quantity = quantity,
                    AddedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            var cartCount = cart.Items.Sum(i => i.Quantity);
            return Json(new
            {
                success = true,
                message = "Fridge added to cart successfully!",
                cartCount = cartCount
            });
        }

        private JsonResult AddToCookieCart(int fridgeId, int quantity)
        {
            var fridge = _context.Fridges
                .FirstOrDefault(f => f.FridgeId == fridgeId && f.Status == "Available" && f.IsActive);

            if (fridge == null)
            {
                return Json(new { success = false, message = "Fridge is no longer available" });
            }

            CartHelper.AddToCart(fridgeId, quantity, Request, Response);

            var cartCount = CartHelper.GetCartSize(Request, Response);
            return Json(new
            {
                success = true,
                message = "Fridge added to cart successfully!",
                cartCount = cartCount
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int fridgeId, int quantity)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = await _context.ShoppingCart
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart != null)
                    {
                        var item = cart.Items.FirstOrDefault(i => i.FridgeId == fridgeId);
                        if (item != null)
                        {
                            if (quantity <= 0)
                            {
                                cart.Items.Remove(item);
                            }
                            else
                            {
                                item.Quantity = quantity;
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    if (quantity <= 0)
                    {
                        CartHelper.RemoveFromCart(fridgeId, Request, Response);
                    }
                    else
                    {
                        CartHelper.UpdateCartQuantity(fridgeId, quantity, Request, Response);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity");
                TempData["ErrorMessage"] = "Error updating quantity";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int fridgeId)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = await _context.ShoppingCart
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart != null)
                    {
                        var item = cart.Items.FirstOrDefault(i => i.FridgeId == fridgeId);
                        if (item != null)
                        {
                            cart.Items.Remove(item);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    CartHelper.RemoveFromCart(fridgeId, Request, Response);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                TempData["ErrorMessage"] = "Error removing item";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public JsonResult GetCartCount()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { count = 0 });
                }

                var cart = _context.ShoppingCart
                    .Include(c => c.Items)
                    .FirstOrDefault(c => c.UserId == userId);

                var count = cart?.Items.Sum(i => i.Quantity) ?? 0;
                return Json(new { count });
            }
            else
            {
                var count = CartHelper.GetCartSize(Request, Response);
                return Json(new { count });
            }
        }
    }
}