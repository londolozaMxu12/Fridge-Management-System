using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeManagementSystem.Services
{
    public class OrderBusinessRules
    {
        private readonly FridgeManagementSystemContext _context;
        //private readonly ILogger<OrderBusinessRules> _logger;

        public OrderBusinessRules(FridgeManagementSystemContext context/*, ILogger<OrderBusinessRules> logger*/)
        {
            _context = context;
           // _logger = logger;
        }

        public async Task ApplyBusinessRulesAsync(Order order, string newPaymentStatus, string newOrderStatus)
        {
            var currentPaymentStatus = order.PaymentStatus?.ToLower();
            var currentOrderStatus = order.OrderStatus;
            var newPaymentStatusLower = newPaymentStatus?.ToLower();

            // Rule 1: Auto-advance order when payment is accepted
            if (newPaymentStatusLower == "accepted" && currentPaymentStatus != "accepted")
            {
                if (currentOrderStatus == "Received")
                {
                    order.OrderStatus = "Accepted";
                    //_logger.LogInformation("Order {OrderId} auto-advanced to 'Accepted' after payment acceptance", order.Id);
                }

                // Generate invoice when payment is accepted
                await GenerateInvoiceAsync(order);
            }

            // Rule 2: Auto-cancel order when payment fails or is cancelled
            if ((newPaymentStatusLower == "failed" || newPaymentStatusLower == "cancelled")
                && currentOrderStatus != "Cancelled" && currentOrderStatus != "Delivered")
            {
                order.OrderStatus = "Cancelled";
                //_logger.LogInformation("Order {OrderId} auto-cancelled due to payment status: {PaymentStatus}",
                   // order.Id, newPaymentStatus);

                await FreeUpAllocatedFridgesAsync(order.Id);
            }

            // Rule 3: Update fridge status based on order status
            await UpdateFridgeStatusAsync(order, newOrderStatus, currentOrderStatus);

            // Rule 4: Prevent order acceptance if payment not accepted
            if (newOrderStatus == "Accepted" && currentPaymentStatus != "accepted")
            {
                throw new InvalidOperationException("Cannot accept order without accepted payment.");
            }
        }

        private async Task GenerateInvoiceAsync(Order order)
        {
            try
            {
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.OrderId == order.Id);

                if (existingInvoice == null)
                {
                    // Create a logger specifically for InvoiceService
                    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                    var invoiceServiceLogger = loggerFactory.CreateLogger<InvoiceService>();

                    var invoiceService = new InvoiceService(_context);
                    await invoiceService.GenerateInvoiceAsync(order.Id);
                   // _logger.LogInformation("Invoice generated for order {OrderId}", order.Id);
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error generating invoice for order {OrderId}", order.Id);
            }
        }

        private async Task FreeUpAllocatedFridgesAsync(int orderId)
        {
            try
            {
                var allocations = await _context.Allocations
                    .Where(a => a.OrderId == orderId && a.IsActive)
                    .ToListAsync();

                foreach (var allocation in allocations)
                {
                    allocation.IsActive = false;
                }

                await _context.SaveChangesAsync();
               // _logger.LogInformation("Freed up allocations for order {OrderId}", orderId);
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error freeing up allocations for order {OrderId}", orderId);
            }
        }

        private async Task UpdateFridgeStatusAsync(Order order, string newOrderStatus, string currentOrderStatus)
        {
            try
            {
                if (newOrderStatus == "Shipped" && currentOrderStatus != "Shipped")
                {
                    await UpdateFridgesToStatusAsync(order, "Shipped");
                   // _logger.LogInformation("Order {OrderId} shipped, fridge status updated", order.Id);
                }
                else if (newOrderStatus == "Delivered" && currentOrderStatus != "Delivered")
                {
                    await UpdateFridgesToStatusAsync(order, "Delivered");
                   // _logger.LogInformation("Order {OrderId} delivered, fridge status updated", order.Id);
                }
                else if (newOrderStatus == "Returned" && currentOrderStatus != "Returned")
                {
                    await UpdateFridgesToStatusAsync(order, "Available");
                    await FreeUpAllocatedFridgesAsync(order.Id);
                   // _logger.LogInformation("Order {OrderId} returned, fridges returned to inventory", order.Id);
                }
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error updating fridge status for order {OrderId}", order.Id);
            }
        }

        private async Task UpdateFridgesToStatusAsync(Order order, string status)
        {
            var fridgeIds = order.Items.Select(i => i.FridgeId).ToList();
            var fridges = await _context.Fridges
                .Where(f => fridgeIds.Contains(f.FridgeId))
                .ToListAsync();

            foreach (var fridge in fridges)
            {
                fridge.Status = status;
            }

            await _context.SaveChangesAsync();
            //_logger.LogInformation("Updated {Count} fridges to status {Status} for order {OrderId}",
                //fridges.Count, status, order.Id);
        }
    }
}