using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerOrdersController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly ILogger<CustomerOrdersController> _logger;
        private readonly IInvoiceService _invoice;

        public CustomerOrdersController(FridgeManagementSystemContext context, ILogger<CustomerOrdersController> logger, IInvoiceService invoice)
        {
            _context = context;
            _logger = logger;
            _invoice = invoice;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                var query = _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .Include(o => o.Invoices) 
                    .Where(o => o.CustomerId == userId)
                    .OrderByDescending(o => o.CreatedAt);

                var totalCount = await query.CountAsync();
                var pageSize = 5;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Ensure pageIndex is within valid range
                pageIndex = Math.Max(1, Math.Min(pageIndex, totalPages > 0 ? totalPages : 1));

                var orders = await query
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.PageIndex = pageIndex;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading orders for customer");
                TempData["Error"] = "An error occurred while loading your orders.";
                return View(new List<Order>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                var order = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);

                if (order == null)
                {
                    TempData["Error"] = "Order not found or you don't have permission to view it.";
                    return RedirectToAction(nameof(Index));
                }

                // Calculate order totals for the view
                ViewBag.Subtotal = order.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
                ViewBag.TotalAmount = ViewBag.Subtotal + order.ShippingFee;
                ViewBag.TotalUnits = order.Items?.Sum(i => i.Quantity) ?? 0;
                ViewBag.DistinctItems = order.Items?.Count ?? 0;

                return View(order);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading order details. OrderId: {OrderId}", id);
                TempData["Error"] = "An error occurred while loading order details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get the order with ALL related data
                var order = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(oi => oi.Fridge)
                    .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Check if order can be cancelled
                var cancellableStatuses = new[] { "Received", "Pending", "Processing" };
                if (!cancellableStatuses.Contains(order.OrderStatus))
                {
                    return Json(new
                    {
                        success = false,
                        message = $"This order cannot be cancelled because it's already {order.OrderStatus}."
                    });
                }

                // Update order status
                order.OrderStatus = "Cancelled";

                // Release all fridges from order items
                if (order.Items != null && order.Items.Any())
                {
                    foreach (var item in order.Items)
                    {
                        if (item.Fridge != null)
                        {
                            // Reset fridge status and clear customer association
                            item.Fridge.Status = "Available";
                            item.Fridge.CustomerId = null;
                            item.Fridge.AllocationDate = null;

                            _logger?.LogInformation("Released fridge {FridgeId} from order {OrderId}", item.Fridge.FridgeId, order.Id);
                        }
                    }
                }
                else
                {
                    _logger?.LogWarning("Order {OrderId} has no order items - this might indicate a data issue", id);
                }

                // Remove any allocations for this order
                var allocations = await _context.Allocations
                    .Where(a => a.OrderId == id && a.IsActive)
                    .ToListAsync();

                foreach (var allocation in allocations)
                {
                    allocation.IsActive = false;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Order cancelled successfully. All items have been released."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger?.LogError(ex, "Error cancelling order {OrderId}", id);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while cancelling the order. Please try again."
                });
            }
        }

        //method to get order tracking information
        public async Task<IActionResult> TrackOrder(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var order = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Safely handle items - fixed version
                var items = (order.Items ?? new List<OrderItem>()).Select(i => new
                {
                    Name = i.Fridge?.FridgeType?.Name ?? "Unknown Product",
                    Brand = i.Fridge?.FridgeType?.Brand ?? "Unknown Brand",
                    Model = i.Fridge?.FridgeType?.Model ?? "Unknown Model",
                    Quantity = i.Quantity
                }).ToList();

                var trackingInfo = new
                {
                    OrderId = order.Id,
                    Status = order.OrderStatus ?? "Unknown",
                    PaymentStatus = order.PaymentStatus ?? "Unknown",
                    CreatedAt = order.CreatedAt,
                    EstimatedDelivery = order.CreatedAt.AddDays(7),
                    Items = items
                };

                return Json(new { success = true, data = trackingInfo });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in TrackOrder for order {OrderId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Method to get order items with details
        public async Task<IActionResult> GetOrderItems(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == id && oi.Order.CustomerId == userId)
                    .Include(oi => oi.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .Select(oi => new
                    {
                        OrderItemId = oi.Id,
                        FridgeId = oi.FridgeId,
                        FridgeName = oi.Fridge.FridgeType.Name,
                        Brand = oi.Fridge.FridgeType.Brand,
                        Model = oi.Fridge.FridgeType.Model,
                        SerialNumber = oi.Fridge.SerialNumber,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice,
                        FridgeStatus = oi.Fridge.Status,
                        ImageFileName = oi.Fridge.ImageFileName
                    })
                    .ToListAsync();

                return Json(new { success = true, items = orderItems });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading order items for order {OrderId}", id);
                return Json(new { success = false, message = "Error loading order items." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Verify the order belongs to the current user
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);

                if (order == null)
                {
                    TempData["Error"] = "Order not found or you don't have permission to access it.";
                    return RedirectToAction(nameof(Index));
                }

                // Generate or get existing invoice
                var invoice = await _invoice.GenerateInvoiceAsync(id);
                var pdfBytes = await _invoice.GetInvoicePdfAsync(invoice.Id);

                return File(pdfBytes, "application/pdf", $"{invoice.InvoiceNumber}.pdf");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating invoice for order {OrderId}", id);
                TempData["Error"] = "An error occurred while generating the invoice.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewInvoice(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Order not found." });
                }

                // Generate or get existing invoice
                var invoice = await _invoice.GenerateInvoiceAsync(id);
                var pdfBytes = await _invoice.GetInvoicePdfAsync(invoice.Id);
                var base64Pdf = Convert.ToBase64String(pdfBytes);

                return Json(new { success = true, data = base64Pdf });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating invoice for order {OrderId}", id);
                return Json(new { success = false, message = "Error generating invoice." });
            }
        }

        //method to view invoice details
        public async Task<IActionResult> InvoiceDetails(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var invoice = await _context.Invoices
                    .Include(i => i.Order)
                    .Include(i => i.InvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == id && i.Order.CustomerId == userId);

                if (invoice == null)
                {
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(invoice);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading invoice {InvoiceId}", id);
                TempData["Error"] = "Error loading invoice details.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}