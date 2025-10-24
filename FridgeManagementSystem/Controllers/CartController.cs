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

        public CartController(FridgeManagementSystemContext context,
                            UserManager<ApplicationUser> userManager,
                            IConfiguration configuration,
                            ILogger<CartController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _shippingFee = configuration.GetValue<decimal>("CartSettings:ShippingFee", 0);
        }

        public IActionResult Index()
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
                    var user = _userManager.GetUserAsync(User).Result;
                    var model = new CheckoutViewModel
                    {
                        DeliveryAddress = user?.Address ??", "+ user?.Suburb ?? ", " + user?.City ?? ""
                    };
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
        public async Task<IActionResult> Confirm(int any)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                List<CartItemViewModel> cartItems;
                List<CartItem> dbCartItems = new List<CartItem>();

                if (User.Identity.IsAuthenticated)
                {
                    var cart = await _context.ShoppingCart
                        .Include(c => c.Items)
                        .ThenInclude(i => i.Fridge)
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
                        Fridge = item.Fridge
                    }).ToList();

                    dbCartItems = cart.Items.ToList();
                }
                else
                {
                    cartItems = CartHelper.GetCartItems(Request, Response, _context);
                }

                string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
                string paymentMethod = TempData["PaymentMethod"] as string ?? "";
                string notes = TempData["Notes"] as string ?? "";

                if (!cartItems.Any() || deliveryAddress.Length == 0 || paymentMethod.Length == 0)
                {
                    return RedirectToAction("Index", "Home");
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

                // Add order items
                foreach (var item in cartItems)
                {
                    order.Items.Add(new OrderItem
                    {
                        FridgeId = item.FridgeId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });

                    // Update fridge status to allocated
                    var fridge = await _context.Fridges.FindAsync(item.FridgeId);
                    if (fridge != null)
                    {
                        fridge.Status = "Allocated";
                        fridge.AllocationDate = DateTime.UtcNow;

                        // Find customer and associate with fridge
                        var customer = await _context.Customers
                            .FirstOrDefaultAsync(c => c.UserId == userId);
                        if (customer != null)
                        {
                            fridge.CustomerId = customer.Id;
                        }
                    }
                }

                _context.Orders.Add(order);

                // Clear the cart
                if (User.Identity.IsAuthenticated)
                {
                    // Clear database cart
                    var cart = await _context.ShoppingCart
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart != null)
                    {
                        _context.CartItems.RemoveRange(cart.Items);
                    }
                }
                else
                {
                    // Clear cookie cart
                    CartHelper.ClearCart(Response);
                }

                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Order created successfully!";
                ViewBag.OrderId = order.Id;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order");
                ViewBag.ErrorMessage = "An error occurred while creating your order";
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