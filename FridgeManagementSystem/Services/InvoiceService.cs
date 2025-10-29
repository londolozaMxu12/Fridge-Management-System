using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeManagementSystem.Services
{
    
    public class InvoiceService : IInvoiceService
    {
        private readonly FridgeManagementSystemContext _context;

        public InvoiceService(FridgeManagementSystemContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<Invoice> GenerateInvoiceAsync(int orderId)
        {
            // Check if invoice already exists
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.OrderId == orderId);

            if (existingInvoice != null)
            {
                return existingInvoice;
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new ArgumentException("Order not found");

            // Create invoice
            var invoice = new Invoice
            {
                OrderId = orderId,
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.Now.AddDays(7),
                Subtotal = order.Items.Sum(i => i.Quantity * i.UnitPrice),
                ShippingFee = order.ShippingFee,
                TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice) + order.ShippingFee,
                Status = order.PaymentStatus?.ToLower() == "accepted" ? "Paid" : "Pending",
                CreatedAt = DateTime.Now
            };

            // Add invoice items
            foreach (var orderItem in order.Items)
            {
                var invoiceItem = new InvoiceItem
                {
                    FridgeId = orderItem.FridgeId,
                    ProductName = orderItem.Fridge?.FridgeType?.Name ?? "Unknown Product",
                    Brand = orderItem.Fridge?.FridgeType?.Brand ?? "Unknown Brand",
                    Model = orderItem.Fridge?.FridgeType?.Model ?? "Unknown Model",
                    Quantity = orderItem.Quantity,
                    UnitPrice = orderItem.UnitPrice,
                    TotalPrice = orderItem.Quantity * orderItem.UnitPrice
                };
                invoice.InvoiceItems.Add(invoiceItem);
            }

            // Generate and store PDF
            var pdfBytes = await GeneratePdfBytes(invoice, order);
            invoice.PdfData = pdfBytes;
            invoice.PdfFileName = $"{invoice.InvoiceNumber}.pdf";

            // Save to database
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task<Invoice> GetInvoiceByOrderIdAsync(int orderId)
        {
            return await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.OrderId == orderId);
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Order)
                    .ThenInclude(o => o.Customer)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
                throw new ArgumentException("Invoice not found");

            return await GeneratePdfBytes(invoice, invoice.Order);
        }

        public async Task<byte[]> GetInvoicePdfAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice?.PdfData == null)
            {
                // If no stored PDF, generate a new one
                return await GenerateInvoicePdfAsync(invoiceId);
            }

            return invoice.PdfData;
        }

        private async Task<byte[]> GeneratePdfBytes(Invoice invoice, Order order)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .AlignCenter()
                        .Text("INVOICE")
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Invoice Details
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Invoice #: {invoice.InvoiceNumber}");
                                    col.Item().Text($"Order #: {order.Id}");
                                    col.Item().Text($"Issue Date: {invoice.IssueDate:MMMM dd, yyyy}");
                                    col.Item().Text($"Due Date: {invoice.DueDate:MMMM dd, yyyy}");
                                    col.Item().Text($"Status: {invoice.Status}");
                                });

                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                    col.Item().Text(invoice.CompanyName).SemiBold();
                                    col.Item().Text(invoice.CompanyAddress);
                                    col.Item().Text($"Phone: {invoice.CompanyPhone}");
                                    col.Item().Text($"Email: {invoice.CompanyEmail}");
                                });
                            });

                            // Customer Information
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("BILL TO").SemiBold();
                                    col.Item().Text(order.Customer?.User?.FullName ?? "N/A");
                                    col.Item().Text(order.Customer?.BusinessName ?? "N/A");
                                    col.Item().Text(order.DeliveryAddress);
                                    col.Item().Text($"Email: {order.Customer?.User?.Email ?? "N/A"}");
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("DELIVERED TO").SemiBold();
                                    col.Item().Text(order.Customer?.User?.FullName ?? "N/A");
                                    col.Item().Text(order.DeliveryAddress);
                                });
                            });

                            // Items Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25);
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(90);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("#");
                                    header.Cell().Element(CellStyle).Text("Description");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Qty");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Unit Price");
                                    header.Cell().Element(CellStyle).AlignRight().Text("Total");

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container
                                            .DefaultTextStyle(x => x.SemiBold())
                                            .PaddingVertical(5)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Black);
                                    }
                                });

                                int itemNumber = 1;
                                foreach (var item in invoice.InvoiceItems)
                                {
                                    table.Cell().Element(CellStyle).Text(itemNumber.ToString());
                                    table.Cell().Element(CellStyle).Text($"{item.ProductName}\n{item.Brand} {item.Model}");
                                    table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                                    table.Cell().Element(CellStyle).AlignRight().Text($"R {item.UnitPrice:N2}");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"R {item.TotalPrice:N2}");
                                    itemNumber++;

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                }
                            });

                            // Summary
                            column.Item().AlignRight().Column(col =>
                            {
                                col.Spacing(5);

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem();
                                    row.ConstantColumn(150);
                                    row.ConstantColumn(120);
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Subtotal:");
                                    row.ConstantColumn(150);
                                    row.ConstantColumn(120).AlignRight().Text($"R {invoice.Subtotal:N2}");
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Shipping Fee:");
                                    row.ConstantColumn(150);
                                    row.ConstantColumn(120).AlignRight().Text($"R {invoice.ShippingFee:N2}");
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Total Amount:").SemiBold();
                                    row.ConstantColumn(150);
                                    row.ConstantColumn(120).AlignRight().Text($"R {invoice.TotalAmount:N2}").SemiBold();
                                });
                            });

                            // Terms and Conditions
                            column.Item().PaddingTop(25).Column(col =>
                            {
                                col.Item().Text("Terms & Conditions").SemiBold();
                                col.Item().Text("Payment is due within 7 days. Thank you for choosing us!");
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month.ToString("D2");

            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}{month}"))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            var sequence = 1;
            if (lastInvoice != null)
            {
                var lastSequence = int.Parse(lastInvoice.InvoiceNumber.Split('-').Last());
                sequence = lastSequence + 1;
            }

            return $"INV-{year}{month}-{sequence:D4}";
        }
    }
}
