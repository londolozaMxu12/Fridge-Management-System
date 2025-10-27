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
    [Authorize] 
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
                            ILogger<CartController> logger,
                            IOrderNotificationRepository notification)
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

        [Authorize]
        public async Task<IActionResult> IndexAsync()
        {
            try
            {
                // Get cart items from database for logged-in users
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cart = await _context.ShoppingCart
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                List<CartItemViewModel> cartItems;
                decimal subtotal;

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

                ViewBag.CartItems = cartItems;
                ViewBag.ShippingFee = _shippingFee;
                ViewBag.Subtotal = subtotal;
                ViewBag.Total = subtotal + _shippingFee;

                // Pre-fill the form with user's address
                var user = await _userManager.GetUserAsync(User);
                var model = new CheckoutViewModel();

                if (user != null)
                {
                    model.DeliveryAddress = BuildDeliveryAddress(user);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart page");
                ViewBag.ErrorMessage = "An error occurred while loading your cart";
                return View(new CheckoutViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IndexAsync(CheckoutViewModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cart = await _context.ShoppingCart
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                List<CartItemViewModel> cartItems;
                decimal subtotal;

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

                ViewBag.CartItems = cartItems;
                ViewBag.ShippingFee = _shippingFee;
                ViewBag.Subtotal = subtotal;
                ViewBag.Total = subtotal + _shippingFee;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check if shopping cart is empty
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
        public async Task<IActionResult> ConfirmAsync()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

                var cartItems = cart.Items.Select(item => new CartItemViewModel
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

                var subtotal = cartItems.Sum(item => item.Quantity * item.UnitPrice);
                var cartSize = cart.Items.Sum(i => i.Quantity);

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
        public async Task<IActionResult> ConfirmAsync(string deliveryAddress, string paymentMethod)
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

                var cartItems = cart.Items.Select(item => new CartItemViewModel
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
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();

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
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddToCartAsync(int fridgeId, int quantity = 1)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding fridge to cart. FridgeId: {FridgeId}", fridgeId);
                return Json(new { success = false, message = "An error occurred while adding to cart" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantityAsync(int fridgeId, int quantity)
        {
            try
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItemAsync(int fridgeId)
        {
            try
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
        public async Task<JsonResult> GetCartCount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { count = 0 });
                }

                var cart = await _context.ShoppingCart
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                var count = cart?.Items.Sum(i => i.Quantity) ?? 0;
                return Json(new { count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return Json(new { count = 0 });
            }
        }
    }
}