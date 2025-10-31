using iTextSharp.text;
using iTextSharp.text.pdf;
using FridgeManagementSystem.Models;
using System.Collections.Generic;
using System.IO;

namespace FridgeManagementSystem.Services
{
    public interface IPdfReportService
    {
        byte[] GenerateAllocationHistoryPdf(IEnumerable<Allocation> allocations);
    }

    public class PdfReportService : IPdfReportService
    {
        public byte[] GenerateAllocationHistoryPdf(IEnumerable<Allocation> allocations)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Create document
                var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Add title and header
                AddHeader(document);

                // Add summary section
                AddSummarySection(document, allocations);

                // Add table
                AddAllocationTable(document, allocations);

                // Add footer
                AddFooter(document);

                document.Close();
                return memoryStream.ToArray();
            }
        }

        private void AddHeader(Document document)
        {
            // Company header with blue background
            var headerTable = new PdfPTable(1);
            headerTable.WidthPercentage = 100;
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
            headerTable.DefaultCell.BackgroundColor = new BaseColor(52, 152, 219);
            headerTable.DefaultCell.Padding = 10;
            headerTable.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;

            // Company name
            var companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.WHITE);
            var companyName = new Paragraph("Fridge Management System", companyFont);
            headerTable.AddCell(new PdfPCell(companyName) { Border = Rectangle.NO_BORDER, PaddingBottom = 5 });

            // Report title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.WHITE);
            var reportTitle = new Paragraph("Allocation History Report", titleFont);
            headerTable.AddCell(new PdfPCell(reportTitle) { Border = Rectangle.NO_BORDER });

            // Report date
            var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.WHITE);
            var reportDate = new Paragraph($"Generated on: {DateTime.Now:MMMM dd, yyyy 'at' hh:mm tt}", dateFont);
            headerTable.AddCell(new PdfPCell(reportDate) { Border = Rectangle.NO_BORDER, PaddingTop = 5 });

            document.Add(headerTable);
            document.Add(new Paragraph(" ")); // Spacing
        }

        private void AddSummarySection(Document document, IEnumerable<Allocation> allocations)
        {
            // Calculate statistics
            var totalAllocations = allocations.Count();
            var activeAllocations = allocations.Count(a => a.IsActive);
            var uniqueCustomers = allocations.Select(a => a.CustomerId).Distinct().Count();
            var uniqueFridges = allocations.Select(a => a.FridgeId).Distinct().Count();

            // Create summary table
            var summaryTable = new PdfPTable(4);
            summaryTable.WidthPercentage = 100;
            summaryTable.DefaultCell.Padding = 8;
            summaryTable.DefaultCell.BackgroundColor = new BaseColor(245, 245, 245);
            summaryTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var summaryFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            summaryTable.AddCell(new PdfPCell(new Phrase($"Total Allocations: {totalAllocations}", summaryFont)) { Border = Rectangle.NO_BORDER });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Active: {activeAllocations}", summaryFont)) { Border = Rectangle.NO_BORDER });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Unique Customers: {uniqueCustomers}", summaryFont)) { Border = Rectangle.NO_BORDER });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Unique Fridges: {uniqueFridges}", summaryFont)) { Border = Rectangle.NO_BORDER });

            document.Add(summaryTable);
            document.Add(new Paragraph(" ")); // Spacing
        }

        private void AddAllocationTable(Document document, IEnumerable<Allocation> allocations)
        {
            // Create table with 7 columns
            var table = new PdfPTable(7);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 15f, 10f, 20f, 15f, 25f, 15f, 10f });

            // Table headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
            var headerBackground = new BaseColor(52, 152, 219);

            string[] headers = { "Allocation Date", "Order ID", "Customer", "Business", "Fridge Details", "Allocated By", "Status" };

            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = headerBackground,
                    Padding = 5,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
            }

            // Table data
            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);
            var alternateBackground = new BaseColor(250, 250, 250);

            int rowCount = 0;
            foreach (var allocation in allocations)
            {
                var backgroundColor = rowCount % 2 == 0 ? BaseColor.WHITE : alternateBackground;

                // Allocation Date
                AddTableCell(table, allocation.AllocationDate.ToString("MM/dd/yyyy HH:mm"), dataFont, backgroundColor);

                // Order ID
                AddTableCell(table, allocation.OrderId?.ToString() ?? "N/A", dataFont, backgroundColor);

                // Customer
                var customerName = allocation.Customer?.User?.FullName ?? "N/A";
                var customerEmail = allocation.Customer?.User?.Email ?? "";
                var customerText = string.IsNullOrEmpty(customerEmail) ? customerName : $"{customerName}\n{customerEmail}";
                AddTableCell(table, customerText, dataFont, backgroundColor);

                // Business
                var businessName = allocation.Customer?.BusinessName ?? "N/A";
                var customerType = allocation.Customer?.CustomerType ?? "";
                var businessText = string.IsNullOrEmpty(customerType) ? businessName : $"{businessName}\n{customerType}";
                AddTableCell(table, businessText, dataFont, backgroundColor);

                // Fridge Details
                var fridgeType = allocation.Fridge?.FridgeType?.Name ?? "N/A";
                var brand = allocation.Fridge?.FridgeType?.Brand ?? "N/A";
                var model = allocation.Fridge?.FridgeType?.Model ?? "N/A";
                var serial = allocation.Fridge?.SerialNumber ?? "N/A";
                var fridgeText = $"{fridgeType}\n{brand} - {model}\nSerial: {serial}\nFridge ID: {allocation.FridgeId}";
                AddTableCell(table, fridgeText, dataFont, backgroundColor);

                // Allocated By
                var allocatedByName = allocation.AllocatedBy?.FullName ?? "System";
                var allocatedByEmail = allocation.AllocatedBy?.Email ?? "";
                var allocatedByText = string.IsNullOrEmpty(allocatedByEmail) ? allocatedByName : $"{allocatedByName}\n{allocatedByEmail}";
                AddTableCell(table, allocatedByText, dataFont, backgroundColor);

                // Status
                var status = allocation.IsActive ? "Active" : "Inactive";
                var statusColor = allocation.IsActive ? new BaseColor(40, 167, 69) : new BaseColor(108, 117, 125);
                var statusFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);

                var statusCell = new PdfPCell(new Phrase(status, statusFont))
                {
                    BackgroundColor = statusColor,
                    Padding = 4,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                table.AddCell(statusCell);

                rowCount++;
            }

            document.Add(table);
        }

        private void AddTableCell(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                Padding = 4,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            table.AddCell(cell);
        }

        private void AddFooter(Document document)
        {
            document.Add(new Paragraph(" ")); // Spacing

            var footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY);
            var footer = new Paragraph("Fridge Management System - Confidential Report", footerFont);
            document.Add(footer);

            var supportText = new Paragraph("Support: support@fridgemanagement.com", footerFont);
            supportText.Alignment = Element.ALIGN_RIGHT;
            document.Add(supportText);
        }
    }
}
