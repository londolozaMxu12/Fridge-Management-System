using FridgeManagementSystem.Areas.Identity.Data;

public class OrderNotificationRepository : IOrderNotificationRepository
{
    private readonly FridgeManagementSystemContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<OrderNotificationRepository> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderNotificationRepository(
        FridgeManagementSystemContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<OrderNotificationRepository> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }
    // Helper method to generate absolute URLs
    private string GenerateAbsoluteUrl(string relativePath)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null) return relativePath;

        // Build absolute URL
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        return $"{baseUrl}{relativePath}";
    }

    public async Task NotifyCustomerAboutOrderUpdate(Order order, string previousStatus)
    {
        try
        {
            string message = "";
            string title = "Order Status Updated";

            // Customize message based on the status change
            switch (order.OrderStatus?.ToLower())
            {
                case "accepted":
                    message = $"Your order #{order.Id} has been accepted and is being processed. " +
                             $"A fridge will be allocated to you shortly.";
                    title = "Order Accepted!";
                    break;

                case "processing":
                    message = $"Your order #{order.Id} is now being processed. " +
                             $"We're preparing your items for shipment.";
                    break;

                case "shipped":
                    message = $"Great news! Your order #{order.Id} has been shipped. " +
                             $"Your fridge is on its way to you.";
                    title = "Order Shipped!";
                    break;

                case "delivered":
                    message = $"Your order #{order.Id} has been delivered. " +
                             $"Thank you for choosing NM Design Hub!";
                    title = "Order Delivered!";
                    break;

                case "cancelled":
                    message = $"Your order #{order.Id} has been cancelled. " +
                             $"Please reorder.";
                    title = "Order Cancelled";
                    break;

                case "returned":
                    message = $"Your order #{order.Id} has been marked as returned. " +
                             $"Our team will process your return shortly.";
                    break;

                default:
                    message = $"Your order #{order.Id} status has been updated from {previousStatus} to {order.OrderStatus}.";
                    break;
            }

            // Add payment status information if relevant
            if (order.PaymentStatus?.ToLower() == "accepted" && order.OrderStatus?.ToLower() != "accepted")
            {
                message += " Your payment has been successfully processed.";
            }

            var notification = new Notification
            {
                UserId = order.CustomerId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now,
                Link = GenerateAbsoluteUrl($"/CustomerOrders/Details/{order.Id}")
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Notification sent to customer {order.CustomerId} for order {order.Id} status change: {previousStatus} -> {order.OrderStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating order update notification for order {order.Id}");
        }
    }
    public async Task NotifyAboutFridgeAllocation(Order order, string allocatedBy, string customerId)
    {
        try
        {
            var orderWithDetails = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (orderWithDetails == null)
            {
                _logger.LogWarning($"Order {order.Id} not found for allocation notification");
                return;
            }

            // Get allocated fridge details
            var allocatedFridges = orderWithDetails.Items?
                .Select(i => i.Fridge)
                .Where(f => f != null && f.Status == "Allocated")
                .ToList() ?? new List<Fridge>();

            var fridgeNames = allocatedFridges
                .Select(f => f.FridgeType?.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();

            var fridgeSummary = fridgeNames.Any()
                ? string.Join(", ", fridgeNames)
                : "fridge";

            // Notify the customer
            var customerNotification = new Notification
            {
                UserId = customerId,
                Title = "Fridge Allocated",
                Message = $"Your {fridgeSummary} Fridge has been allocated. " +
                         $"The installation team will contact you shortly.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Link = GenerateAbsoluteUrl($"/CustomerFridges/Details/{orderWithDetails.Id}")
            };

            _context.Notifications.Add(customerNotification);

            // Notify all customer liaisons using EmployeeType
            var liaisons = await GetCustomerLiaisonsAsync();
            foreach (var liaison in liaisons)
            {
                var liaisonNotification = new Notification
                {
                    UserId = liaison.Id,
                    Title = "Fridge Allocation Completed",
                    Message = $"{fridgeNames.Count} fridge(s) ({fridgeSummary}) " +
                             $"have been allocated to order {orderWithDetails.Id} by {allocatedBy}. " +
                             $"Customer: {orderWithDetails.Customer?.FullName ?? "N/A"}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Link = GenerateAbsoluteUrl($"/Allocation/Details/{orderWithDetails.Id}")
                };

                _context.Notifications.Add(liaisonNotification);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Fridge allocation notifications sent for order {orderWithDetails.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fridge allocation notifications");
        }
    }
    public async Task NotifyAboutStockShortage(int fridgeId, int orderId)
    {
        try
        {
            var fridge = await _context.Fridges
                .Include(f => f.FridgeType)
                .FirstOrDefaultAsync(f => f.FridgeId == fridgeId);

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            // Notify all customer liaisons using EmployeeType
            var liaisons = await GetCustomerLiaisonsAsync();
            foreach (var liaison in liaisons)
            {
                var notification = new Notification
                {
                    UserId = liaison.Id,
                    Title = "Stock Shortage Alert",
                    Message = $"No available {fridge?.FridgeType?.Name ?? "fridge"} in stock for order #{orderId}. " +
                             $"Customer: {order?.Customer?.FullName ?? "N/A"}. " +
                             $"Please check inventory and restock.",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    Link = GenerateAbsoluteUrl($"/LiaisonOrders/Details/{orderId}")
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            _logger.LogWarning($"Stock shortage notifications sent for fridge {fridgeId} on order {orderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stock shortage notifications");
        }
    }

    public async Task NotifyAboutPaymentStatusUpdate(Order order, string previousPaymentStatus)
    {
        try
        {
            string message = "";
            string title = "Payment Status Updated";

            switch (order.PaymentStatus?.ToLower())
            {
                case "accepted":
                    message = $"Your payment for order #{order.Id} has been accepted. " +
                             $"Your order will now be processed.";
                    title = "Payment Accepted!";
                    break;

                case "pending":
                    message = $"Your payment for order #{order.Id} is pending. " +
                             $"We'll notify you once it's processed.";
                    break;

                case "canceled":
                    message = $"Your payment for order #{order.Id} has been cancelled. " +
                             $"Please contact customer support if you need assistance.";
                    title = "Payment Cancelled";
                    break;

                default:
                    message = $"Payment status for order #{order.Id} has been updated from {previousPaymentStatus} to {order.PaymentStatus}.";
                    break;
            }

            var notification = new Notification
            {
                UserId = order.CustomerId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now,
                Link = GenerateAbsoluteUrl($"/CustomerOrders/Details/{order.Id}")
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Payment status notification sent for order {order.Id}: {previousPaymentStatus} -> {order.PaymentStatus}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating payment status notification for order {order.Id}");
        }
    }
    public async Task NotifyLiaisonsAboutNewOrder(Order order)
    {
        try
        {
            var orderWithDetails = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (orderWithDetails == null) return;

            var totalItems = orderWithDetails.Items?.Sum(i => i.Quantity) ?? 0;
            var itemNames = orderWithDetails.Items?
                .Select(i => i.Fridge?.FridgeType?.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();

            var itemsSummary = itemNames.Any()
                ? string.Join(", ", itemNames.Distinct())
                : "items";

            // Get customer liaisons using EmployeeType
            var liaisons = await GetCustomerLiaisonsAsync();

            foreach (var liaison in liaisons)
            {
                var notification = new Notification
                {
                    UserId = liaison.Id,
                    Title = "New Order Received",
                    Message = $"New order #{orderWithDetails.Id} received. " +
                             $"{totalItems} items ({itemsSummary}) waiting for approval.",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    Link = GenerateAbsoluteUrl($"/LiaisonOrders/Details/{orderWithDetails.Id}")
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"New order notification sent to {liaisons.Count} customer liaisons for order {order.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating new order notifications for order {order.Id}");
        }
    }
    public async Task NotifyAboutFreedFridges(Order order, List<int> freedFridgeIds)
    {
        try
        {
            // Get fridge details
            var fridges = await _context.Fridges
                .Include(f => f.FridgeType)
                .Where(f => freedFridgeIds.Contains(f.FridgeId))
                .ToListAsync();

            var fridgeNames = fridges.Select(f => f.FridgeType?.Name).Where(name => !string.IsNullOrEmpty(name));
            var fridgeSummary = string.Join(", ", fridgeNames);

            // Notify the customer
            var customerNotification = new Notification
            {
                UserId = order.CustomerId,
                Title = "Order Cancelled - Fridges Available Again",
                Message = $"Your order #{order.Id} has been cancelled. " +
                         $"The fridge(s) you ordered ({fridgeSummary}) are now available for purchase again.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Link = GenerateAbsoluteUrl($"/CustomerOrders/Index/{order.Id}")
            };

            _context.Notifications.Add(customerNotification);

            // Notify all customer liaisons using EmployeeType
            var liaisons = await GetCustomerLiaisonsAsync();
            foreach (var liaison in liaisons)
            {
                var liaisonNotification = new Notification
                {
                    UserId = liaison.Id,
                    Title = "Fridges Freed from Cancelled Order",
                    Message = $"Order #{order.Id} was cancelled. {freedFridgeIds.Count} fridges ({fridgeSummary}) " +
                             $"are now available in the store inventory.",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    Link = GenerateAbsoluteUrl($"/LiaisonOrders/Details/{order.Id}")
                };

                _context.Notifications.Add(liaisonNotification);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Freed fridge notifications sent for order {order.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating freed fridge notifications for order {order.Id}");
        }
    }
    public async Task NotifyLiaisonsAboutOrderReadyForAllocation(Order order)
    {
        try
        {
            var orderWithDetails = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            if (orderWithDetails == null) return;

            var totalItems = orderWithDetails.Items?.Sum(i => i.Quantity) ?? 0;
            var itemNames = orderWithDetails.Items?
                .Select(i => i.Fridge?.FridgeType?.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();

            var itemsSummary = itemNames.Any()
                ? string.Join(", ", itemNames.Distinct())
                : "items";

            // Notify all customer liaisons using EmployeeType
            var liaisons = await GetCustomerLiaisonsAsync();
            foreach (var liaison in liaisons)
            {
                var notification = new Notification
                {
                    UserId = liaison.Id,
                    Title = "Order Ready for Allocation",
                    Message = $"Order #{orderWithDetails.Id} from {orderWithDetails.Customer?.FullName} " +
                             $"is ready for fridge allocation. {totalItems} {itemsSummary} waiting.",
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    Link = GenerateAbsoluteUrl($"/Allocation/Details/{orderWithDetails.Id}")
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Order ready for allocation notification sent for order {order.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating allocation ready notification for order {order.Id}");
        }
    }

    private async Task<List<ApplicationUser>> GetCustomerLiaisonsAsync()
    {
        try
        {
            // Get all active employees with Customer Liaison type
            var customerLiaisons = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.EmployeeType)
                .Where(e => e.EmployeeType.Name == "CustomerLiaison" && e.IsActive)
                .Select(e => e.User)
                .ToListAsync();

            return customerLiaisons;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer liaisons");
            return new List<ApplicationUser>();
        }
    }
}