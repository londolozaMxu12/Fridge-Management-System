using FridgeManagementSystem.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace FridgeManagementSystem.Repositories
{
    
    public class TechnicianReportRepository : ITechnicianReportRepository
    {
        private readonly FridgeManagementSystemContext _context;

        public TechnicianReportRepository(FridgeManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetActiveTechniciansAsync()
        {
            return await _context.Employees
                .Include(e => e.User)
                .Include(e => e.EmployeeType)
                .Where(e => e.EmployeeType.Name == "FaultTechnician" && e.IsActive)
                .OrderBy(e => e.User.FullName)
                .ToListAsync();
        }

        public async Task<TechnicianReportViewModel> GenerateTechnicianReportAsync(TechnicianReportViewModel parameters)
        {
            if (!parameters.TechnicianId.HasValue)
            {
                throw new ArgumentException("Technician ID is required");
            }

            // Get technician details
            var technician = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == parameters.TechnicianId.Value);

            if (technician == null)
            {
                throw new ArgumentException("Technician not found");
            }

            parameters.TechnicianName = technician.User.FullName;
            parameters.Email = technician.User.Email;
            parameters.JoinDate = technician.User.CreatedAt;

            // Get faults assigned to this technician within date range
            var faultsQuery = _context.Faults
                .Include(f => f.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Include(f => f.RepairSchedules)
                .Where(f => f.FaultTechnicianId == parameters.TechnicianId &&
                           f.ReportedDate >= parameters.StartDate &&
                           f.ReportedDate <= parameters.EndDate);

            var faults = await faultsQuery.ToListAsync();

            var repairSchedules = await _context.RepairSchedules
                .Where(rs => rs.FaultTechnicianId == parameters.TechnicianId &&
                            rs.ScheduledDate >= parameters.StartDate &&
                            rs.ScheduledDate <= parameters.EndDate)
                .ToListAsync();

            // Calculate performance metrics
            parameters.Performance = CalculatePerformanceMetrics(faults, repairSchedules);
            parameters.FaultDetails = GetFaultDetails(faults);
            parameters.MonthlyTrends = await GetMonthlyTrends(parameters.TechnicianId.Value, parameters.StartDate, parameters.EndDate);

            if (parameters.IncludeCharts)
            {
                parameters.FaultsByStatus = GetFaultsByStatus(faults);
                parameters.FaultsByPriority = GetFaultsByPriority(faults);
                parameters.MonthlyCompletionRate = GetMonthlyCompletionRates(parameters.MonthlyTrends);

                // Set the new properties with all statuses and priorities
                parameters.AllStatusesWithCounts = parameters.FaultsByStatus;
                parameters.AllPrioritiesWithCounts = parameters.FaultsByPriority;

            }

            return parameters;
        }

        private TechnicianPerformanceMetrics CalculatePerformanceMetrics(List<Fault> faults, List<RepairSchedule> schedules)
        {
            var completedFaults = faults.Where(f => f.Status == FaultStatus.Completed).ToList();

            // Calculate resolution time using UpdatedAt - ReportedDate for completed faults
            var resolutionTimes = completedFaults
                .Where(f => f.UpdatedAt.HasValue)
                .Select(f => (f.UpdatedAt.Value - f.ReportedDate).TotalDays)
                .ToList();

            var metrics = new TechnicianPerformanceMetrics
            {
                TotalFaults = faults.Count,
                CompletedFaults = completedFaults.Count,
                InProgressFaults = faults.Count(f => f.Status == FaultStatus.InProgress),
                ScheduledFaults = faults.Count(f => f.Status == FaultStatus.Scheduled),
                ReportedFaults = faults.Count(f => f.Status == FaultStatus.Reported),
                CriticalFaults = faults.Count(f => f.Priority == FaultPriority.Critical),
                HighPriorityFaults = faults.Count(f => f.Priority == FaultPriority.High),
                AverageResolutionDays = resolutionTimes.Any() ? (decimal)resolutionTimes.Average() : 0,
                CompletionRate = faults.Count > 0 ? (decimal)completedFaults.Count / faults.Count * 100 : 0,
                TotalScheduledRepairs = schedules.Count,
                CompletedRepairs = schedules.Count(s => s.Status == ScheduleStatus.Completed)
            };

            // Calculate on-time completion (within 7 days for standard faults)
            metrics.OnTimeCompletionRate = CalculateOnTimeCompletionRate(completedFaults);

            return metrics;
        }

        private decimal CalculateOnTimeCompletionRate(List<Fault> completedFaults)
        {
            if (!completedFaults.Any()) return 0;

            var onTimeCompletions = 0;

            foreach (var fault in completedFaults)
            {
                if (fault.UpdatedAt.HasValue)
                {
                    var resolutionDays = (fault.UpdatedAt.Value - fault.ReportedDate).TotalDays;

                    // Define on-time completion based on priority
                    var targetDays = fault.Priority switch
                    {
                        FaultPriority.Critical => 1,   // 1 day for critical
                        FaultPriority.High => 3,       // 3 days for high
                        FaultPriority.Medium => 5,     // 5 days for medium
                        FaultPriority.Low => 7,       // 7 days for low
                        _ => 7
                    };

                    if (resolutionDays <= targetDays)
                    {
                        onTimeCompletions++;
                    }
                }
            }

            return (decimal)onTimeCompletions / completedFaults.Count * 100;
        }

        private List<TechnicianFaultDetail> GetFaultDetails(List<Fault> faults)
        {
            return faults.Select(f => new TechnicianFaultDetail
            {
                FaultId = f.FaultId,
                Title = f.Title,
                Priority = f.Priority.ToString(),
                Status = f.Status.ToString(),
                ReportedDate = f.ReportedDate,
                ScheduledDate = f.ScheduledDate,
                UpdatedAt = f.UpdatedAt,
                ResolutionDays = f.UpdatedAt.HasValue && f.Status == FaultStatus.Completed ?
                    (decimal)(f.UpdatedAt.Value - f.ReportedDate).TotalDays : 0,
                FridgeType = f.Fridge?.FridgeType?.Name ?? "N/A",
                CustomerName = f.ReportedBy?.User?.FullName ?? "N/A",
                ResolutionNotes = f.ResolutionNotes ?? string.Empty,
                OnTimeCompletion = CheckOnTimeCompletion(f)
            }).ToList();
        }

        private bool CheckOnTimeCompletion(Fault fault)
        {
            if (fault.Status != FaultStatus.Completed || !fault.UpdatedAt.HasValue)
                return false;

            var resolutionDays = (fault.UpdatedAt.Value - fault.ReportedDate).TotalDays;

            var targetDays = fault.Priority switch
            {
                FaultPriority.Critical => 1,
                FaultPriority.High => 3,
                FaultPriority.Medium => 5,
                FaultPriority.Low => 7,
                _ => 7
            };

            return resolutionDays <= targetDays;
        }
        private Dictionary<string, int> GetFaultsByStatus(List<Fault> faults)
        {
            // Get all possible statuses from the enum
            var allStatuses = Enum.GetNames(typeof(FaultStatus));
            var result = new Dictionary<string, int>();

            // Initialize all statuses with zero counts
            foreach (var status in allStatuses)
            {
                result[status] = 0;
            }

            // Update with actual counts from faults
            if (faults != null && faults.Any())
            {
                foreach (var fault in faults)
                {
                    var status = fault.Status.ToString();
                    if (result.ContainsKey(status))
                        result[status]++;
                }
            }

            return result;
        }

        private Dictionary<string, int> GetFaultsByPriority(List<Fault> faults)
        {
            // Get all possible priorities from the enum
            var allPriorities = Enum.GetNames(typeof(FaultPriority));
            var result = new Dictionary<string, int>();

            // Initialize all priorities with zero counts
            foreach (var priority in allPriorities)
            {
                result[priority] = 0;
            }

            // Update with actual counts from faults
            if (faults != null && faults.Any())
            {
                foreach (var fault in faults)
                {
                    var priority = fault.Priority.ToString();
                    if (result.ContainsKey(priority))
                        result[priority]++;
                }
            }

            return result;
        }

        private async Task<List<MonthlyPerformance>> GetMonthlyTrends(int technicianId, DateTime startDate, DateTime endDate)
        {
            // First, get the grouped data from database
            var monthlyData = await _context.Faults
                .Where(f => f.FaultTechnicianId == technicianId &&
                           f.ReportedDate >= startDate &&
                           f.ReportedDate <= endDate)
                .GroupBy(f => new { Year = f.ReportedDate.Year, Month = f.ReportedDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalFaults = g.Count(),
                    CompletedFaults = g.Count(f => f.Status == FaultStatus.Completed)
                })
                .ToListAsync(); // Execute the query here

            // complex calculations in memory (client-side)
            var result = new List<MonthlyPerformance>();

            foreach (var monthData in monthlyData)
            {
                // Get the completed faults for this month to calculate average resolution time
                var completedFaults = await _context.Faults
                    .Where(f => f.FaultTechnicianId == technicianId &&
                               f.ReportedDate.Year == monthData.Year &&
                               f.ReportedDate.Month == monthData.Month &&
                               f.Status == FaultStatus.Completed &&
                               f.UpdatedAt.HasValue)
                    .ToListAsync();

                var averageResolutionDays = completedFaults.Any()
                    ? (decimal)completedFaults.Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalDays)
                    : 0;

                var completionRate = monthData.TotalFaults > 0
                    ? (decimal)monthData.CompletedFaults / monthData.TotalFaults * 100
                    : 0;

                result.Add(new MonthlyPerformance
                {
                    Month = $"{monthData.Year}-{monthData.Month:00}",
                    TotalFaults = monthData.TotalFaults,
                    CompletedFaults = monthData.CompletedFaults,
                    CompletionRate = completionRate,
                    AverageResolutionDays = averageResolutionDays
                });
            }

            // If no monthly data, create at least one entry for the current month
            if (!result.Any())
            {
                var currentMonth = $"{DateTime.Now.Year}-{DateTime.Now.Month:00}";
                result.Add(new MonthlyPerformance
                {
                    Month = currentMonth,
                    TotalFaults = 0,
                    CompletedFaults = 0,
                    CompletionRate = 0,
                    AverageResolutionDays = 0
                });
            }

            return result.OrderBy(m => m.Month).ToList();
        }

        private static Dictionary<string, decimal> GetMonthlyCompletionRates(List<MonthlyPerformance> monthlyTrends)
        {
            if (monthlyTrends == null || !monthlyTrends.Any())
                return new Dictionary<string, decimal>();

            return monthlyTrends.ToDictionary(m => m.Month, m => m.CompletionRate);
        }

        // PDF Generation with QuestPDF
        public async Task<byte[]> GenerateTechnicianPdfReportAsync(TechnicianReportViewModel report)
        {
            if (!QuestPDF.Settings.License.HasValue)
            {
                QuestPDF.Settings.License = LicenseType.Community;
            }

            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header()
                            .AlignCenter()
                            .Text("Fault Technician Performance Report")
                            .Bold().FontSize(20).FontColor(Colors.Black);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                // Technician Information
                                column.Item().PaddingBottom(15).Background(Colors.Grey.Lighten3).Padding(10).Column(infoCol =>
                                {
                                    infoCol.Item().Text($"Technician: {report.TechnicianName}").SemiBold();
                                    infoCol.Item().Text($"Email: {report.Email}");
                                    infoCol.Item().Text($"Report Period: {report.StartDate:dd MMM yyyy} to {report.EndDate:dd MMM yyyy}");
                                    infoCol.Item().Text($"Generated On: {DateTime.Now:dd MMM yyyy HH:mm}");
                                });

                                // Performance Summary
                                column.Item().PaddingBottom(10).Text("Performance Summary").SemiBold().FontSize(14);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Total Attended Faults").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Completed").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Avg Resolution").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Completion Rate").FontColor(Colors.White).SemiBold();
                                    });

                                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(report.Performance.TotalFaults.ToString()).SemiBold();
                                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(report.Performance.CompletedFaults.ToString()).SemiBold();
                                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{report.Performance.AverageResolutionDays:F1} days").SemiBold();
                                    table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{report.Performance.CompletionRate:F1}%").SemiBold();
                                });

                                // Detailed Metrics
                                column.Item().PaddingVertical(10).Text("Detailed Metrics").SemiBold().FontSize(14);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Metric").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Value").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Rate").FontColor(Colors.White).SemiBold();
                                    });

                                    AddMetricRow(table, "On-Time Completion", report.Performance.OnTimeCompletionRate.ToString("F1"), "%");
                                    
                                    AddMetricRow(table, "Critical Faults", report.Performance.CriticalFaults.ToString(), "");
                                    AddMetricRow(table, "High Priority Faults", report.Performance.HighPriorityFaults.ToString(), "");
                                    AddMetricRow(table, "In Progress", report.Performance.InProgressFaults.ToString(), "");
                                });

                                // Complete Status Distribution (All Statuses)
                                column.Item().PaddingVertical(10).Text("Fault Distribution by Status").SemiBold().FontSize(14);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Status").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Count").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Percentage").FontColor(Colors.White).SemiBold();
                                    });

                                    var totalFaults = report.Performance.TotalFaults;
                                    var statusOrder = new List<string> { "Reported", "Scheduled", "InProgress", "Completed", "Cancelled" };

                                    foreach (var status in statusOrder)
                                    {
                                        var count = report.AllStatusesWithCounts.ContainsKey(status) ? report.AllStatusesWithCounts[status] : 0;
                                        var percentage = totalFaults > 0 ? (count / (double)totalFaults * 100) : 0;

                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(status);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(count.ToString()).SemiBold();
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{percentage:F1}%");
                                    }
                                });

                                // Complete Priority Distribution (All Priorities)
                                column.Item().PaddingVertical(10).Text("Fault Distribution by Priority").SemiBold().FontSize(14);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Priority").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Count").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Percentage").FontColor(Colors.White).SemiBold();
                                    });

                                    var totalFaults = report.Performance.TotalFaults;
                                    var priorityOrder = new List<string> { "Critical", "High", "Medium", "Low" };

                                    foreach (var priority in priorityOrder)
                                    {
                                        var count = report.AllPrioritiesWithCounts.ContainsKey(priority) ? report.AllPrioritiesWithCounts[priority] : 0;
                                        var percentage = totalFaults > 0 ? (count / (double)totalFaults * 100) : 0;

                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(priority);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(count.ToString()).SemiBold();
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{percentage:F1}%");
                                    }
                                });

                                // Monthly Trends (if available)
                                if (report.MonthlyTrends.Any())
                                {
                                    column.Item().PaddingVertical(10).Text("Monthly Performance Trends").SemiBold().FontSize(14);
                                    column.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Month").FontColor(Colors.White).SemiBold();
                                            header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Total").FontColor(Colors.White).SemiBold();
                                            header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Completed").FontColor(Colors.White).SemiBold();
                                            header.Cell().Background(Colors.Grey.Medium).Padding(5).Text("Rate").FontColor(Colors.White).SemiBold();
                                        });

                                        foreach (var trend in report.MonthlyTrends)
                                        {
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(trend.Month);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(trend.TotalFaults.ToString());
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(trend.CompletedFaults.ToString());
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{trend.CompletionRate:F1}%");
                                        }
                                    });
                                }

                                // Recent Fault Details (Top 5)
                                if (report.FaultDetails.Any())
                                {
                                    column.Item().PaddingVertical(10).Text("Recent Attended Fault Details").SemiBold().FontSize(14);
                                    column.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(60); // ID
                                            columns.RelativeColumn(3);  // Title
                                            columns.RelativeColumn(2);  // Priority
                                            columns.RelativeColumn(2);  // Status
                                            columns.RelativeColumn(2);  // Reported Date
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Grey.Darken1).Padding(5).Text("#").FontColor(Colors.White).SemiBold();
                                            header.Cell().Background(Colors.Grey.Darken1).Padding(5).Text("Title").FontColor(Colors.White).SemiBold();
                                            header.Cell().Background(Colors.Grey.Darken1).Padding(5).Text("Priority").FontColor(Colors.White).SemiBold();
                                            header.Cell().Background(Colors.Grey.Darken1).Padding(5).Text("Status").FontColor(Colors.White).SemiBold();
                                            header.Cell().Background(Colors.Grey.Darken1).Padding(5).Text("Reported").FontColor(Colors.White).SemiBold();
                                        });

                                        foreach (var fault in report.FaultDetails.Take(5))
                                        {
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(fault.FaultId);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(fault.Title.Length > 50 ? fault.Title.Substring(0, 50) + "..." : fault.Title);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(fault.Priority);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(fault.Status);
                                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(fault.ReportedDate.ToString("dd MMM yyyy"));
                                        }
                                    });
                                }

                                // Footer
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
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Generation Error: {ex.Message}");
                throw new Exception("Failed to generate PDF report", ex);
            }
        }

        private void AddMetricRow(TableDescriptor table, string metric, string value, string unit)
        {
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(metric);
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(value).SemiBold();
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(unit);
        }

        // Excel Generation with EPPlus
        public async Task<byte[]> GenerateTechnicianExcelReportAsync(TechnicianReportViewModel report)
        {
            try
            {
                using var package = new ExcelPackage();

                // Summary Sheet
                var summarySheet = package.Workbook.Worksheets.Add("Performance Summary");
                GenerateSummarySheet(summarySheet, report);

                // Fault Details Sheet
                if (report.FaultDetails.Any())
                {
                    var detailsSheet = package.Workbook.Worksheets.Add("Fault Attended Details");
                    GenerateFaultDetailsSheet(detailsSheet, report);
                }

                // Distribution Sheet
                if (report.FaultsByStatus.Any() || report.FaultsByPriority.Any())
                {
                    var distributionSheet = package.Workbook.Worksheets.Add("Distributions");
                    GenerateDistributionSheet(distributionSheet, report);
                }

                // Trends Sheet
                if (report.MonthlyTrends.Any())
                {
                    var trendsSheet = package.Workbook.Worksheets.Add("Monthly Trends");
                    GenerateTrendsSheet(trendsSheet, report);
                }

                return await package.GetAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excel Generation Error: {ex.Message}");
                throw new Exception("Failed to generate Excel report", ex);
            }
        }

        private void GenerateSummarySheet(ExcelWorksheet sheet, TechnicianReportViewModel report)
        {
            // Title
            sheet.Cells[1, 1].Value = "Fault Technician Performance Report";
            sheet.Cells[1, 1].Style.Font.Size = 16;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1, 1, 5].Merge = true;

            // Technician Info
            sheet.Cells[3, 1].Value = "Technician:";
            sheet.Cells[3, 2].Value = report.TechnicianName;
            sheet.Cells[4, 1].Value = "Email:";
            sheet.Cells[4, 2].Value = report.Email;
            sheet.Cells[5, 1].Value = "Report Period:";
            sheet.Cells[5, 2].Value = $"{report.StartDate:dd MMM yyyy} to {report.EndDate:dd MMM yyyy}";
            sheet.Cells[6, 1].Value = "Generated On:";
            sheet.Cells[6, 2].Value = DateTime.Now.ToString("dd MMM yyyy HH:mm");

            // Key Performance Indicators
            var kpiRow = 8;
            sheet.Cells[kpiRow, 1].Value = "Key Performance Indicators";
            sheet.Cells[kpiRow, 1].Style.Font.Size = 14;
            sheet.Cells[kpiRow, 1].Style.Font.Bold = true;
            sheet.Cells[kpiRow, 1, kpiRow, 4].Merge = true;

            kpiRow++;
            CreateKpiTable(sheet, report, kpiRow);

            // Detailed Metrics
            var metricsRow = kpiRow + 6;
            sheet.Cells[metricsRow, 1].Value = "Detailed Metrics";
            sheet.Cells[metricsRow, 1].Style.Font.Size = 14;
            sheet.Cells[metricsRow, 1].Style.Font.Bold = true;
            sheet.Cells[metricsRow, 1, metricsRow, 3].Merge = true;

            metricsRow++;
            CreateMetricsTable(sheet, report, metricsRow);

            // Auto-fit columns
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateKpiTable(ExcelWorksheet sheet, TechnicianReportViewModel report, int startRow)
        {
            // Headers
            sheet.Cells[startRow, 1].Value = "Metric";
            sheet.Cells[startRow, 2].Value = "Value";
            sheet.Cells[startRow, 1, startRow, 2].Style.Font.Bold = true;
            sheet.Cells[startRow, 1, startRow, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[startRow, 1, startRow, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

            // Data
            var data = new[]
            {
        new { Metric = "Total Faults Attended", Value = report.Performance.TotalFaults.ToString() },
        new { Metric = "Faults Completed", Value = report.Performance.CompletedFaults.ToString() },
        new { Metric = "Average Resolution Time", Value = $"{report.Performance.AverageResolutionDays:F1} days" },
        new { Metric = "Completion Rate", Value = $"{report.Performance.CompletionRate:F1}%" },
        new { Metric = "On-Time Completion Rate", Value = $"{report.Performance.OnTimeCompletionRate:F1}%" },
        
    };

            for (int i = 0; i < data.Length; i++)
            {
                sheet.Cells[startRow + i + 1, 1].Value = data[i].Metric;
                sheet.Cells[startRow + i + 1, 2].Value = data[i].Value;

                // Alternate row colors
                if (i % 2 == 0)
                {
                    sheet.Cells[startRow + i + 1, 1, startRow + i + 1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[startRow + i + 1, 1, startRow + i + 1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }
            }

            // Add borders
            var tableRange = sheet.Cells[startRow, 1, startRow + data.Length, 2];
            tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        private void CreateMetricsTable(ExcelWorksheet sheet, TechnicianReportViewModel report, int startRow)
        {
            // Get all status counts for the detailed metrics
            var metrics = new[]
            {
        new { Category = "Fault Status", Metric = "Reported", Value = report.AllStatusesWithCounts["Reported"] },
        new { Category = "Fault Status", Metric = "Scheduled", Value = report.AllStatusesWithCounts["Scheduled"] },
        new { Category = "Fault Status", Metric = "In Progress", Value = report.AllStatusesWithCounts["InProgress"] },
        new { Category = "Fault Status", Metric = "Completed", Value = report.AllStatusesWithCounts["Completed"] },
        new { Category = "Fault Status", Metric = "Cancelled", Value = report.AllStatusesWithCounts["Cancelled"] },
        new { Category = "Priority", Metric = "Critical", Value = report.AllPrioritiesWithCounts["Critical"] },
        new { Category = "Priority", Metric = "High", Value = report.AllPrioritiesWithCounts["High"] },
        new { Category = "Priority", Metric = "Medium", Value = report.AllPrioritiesWithCounts["Medium"] },
        new { Category = "Priority", Metric = "Low", Value = report.AllPrioritiesWithCounts["Low"] }
    };

            // Headers
            sheet.Cells[startRow, 1].Value = "Category";
            sheet.Cells[startRow, 2].Value = "Metric";
            sheet.Cells[startRow, 3].Value = "Count";
            sheet.Cells[startRow, 1, startRow, 3].Style.Font.Bold = true;
            sheet.Cells[startRow, 1, startRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[startRow, 1, startRow, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

            for (int i = 0; i < metrics.Length; i++)
            {
                sheet.Cells[startRow + i + 1, 1].Value = metrics[i].Category;
                sheet.Cells[startRow + i + 1, 2].Value = metrics[i].Metric;
                sheet.Cells[startRow + i + 1, 3].Value = metrics[i].Value;

                // Alternate row colors
                if (i % 2 == 0)
                {
                    sheet.Cells[startRow + i + 1, 1, startRow + i + 1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[startRow + i + 1, 1, startRow + i + 1, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }
            }

            var tableRange = sheet.Cells[startRow, 1, startRow + metrics.Length, 3];
            tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        private void GenerateFaultDetailsSheet(ExcelWorksheet sheet, TechnicianReportViewModel report)
        {
            // Title
            sheet.Cells[1, 1].Value = "Fault Details";
            sheet.Cells[1, 1].Style.Font.Size = 16;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1, 1, 8].Merge = true;

            // Headers
            var headers = new[] { "#", "Title", "Priority", "Status", "Reported Date", "Resolution Days", "Fridge Type", "Customer" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[3, i + 1].Value = headers[i];
                sheet.Cells[3, i + 1].Style.Font.Bold = true;
                sheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            // Data
            for (int i = 0; i < report.FaultDetails.Count; i++)
            {
                var fault = report.FaultDetails[i];
                sheet.Cells[i + 4, 1].Value = fault.FaultId;
                sheet.Cells[i + 4, 2].Value = fault.Title;
                sheet.Cells[i + 4, 3].Value = fault.Priority;
                sheet.Cells[i + 4, 4].Value = fault.Status;
                sheet.Cells[i + 4, 5].Value = fault.ReportedDate;
                sheet.Cells[i + 4, 5].Style.Numberformat.Format = "dd-mm-yyyy";

                sheet.Cells[i + 4, 6].Value = Math.Round(fault.ResolutionDays, 1);
                sheet.Cells[i + 4, 6].Style.Numberformat.Format = "0.0";

                sheet.Cells[i + 4, 7].Value = fault.FridgeType;
                sheet.Cells[i + 4, 8].Value = fault.CustomerName;

                // Alternate row colors
                if (i % 2 == 0)
                {
                    sheet.Cells[i + 4, 1, i + 4, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[i + 4, 1, i + 4, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }
            }

            // Auto-fit and add borders
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            var dataRange = sheet.Cells[3, 1, report.FaultDetails.Count + 3, 8];
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        private void GenerateDistributionSheet(ExcelWorksheet sheet, TechnicianReportViewModel report)
        {
            int currentRow = 1;

            // Complete Status Distribution (All Statuses)
            if (report.AllStatusesWithCounts.Any())
            {
                sheet.Cells[currentRow, 1].Value = "Fault Distribution by Statuses";
                sheet.Cells[currentRow, 1].Style.Font.Size = 14;
                sheet.Cells[currentRow, 1].Style.Font.Bold = true;
                currentRow++;

                sheet.Cells[currentRow, 1].Value = "Status";
                sheet.Cells[currentRow, 2].Value = "Count";
                sheet.Cells[currentRow, 3].Value = "Percentage";
                sheet.Cells[currentRow, 1, currentRow, 3].Style.Font.Bold = true;
                sheet.Cells[currentRow, 1, currentRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[currentRow, 1, currentRow, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                currentRow++;

                var totalFaults = report.Performance.TotalFaults;
                var statusOrder = new List<string> { "Reported", "Scheduled", "InProgress", "Completed", "Cancelled" };

                foreach (var status in statusOrder)
                {
                    var count = report.AllStatusesWithCounts.ContainsKey(status) ? report.AllStatusesWithCounts[status] : 0;
                    var percentage = totalFaults > 0 ? (count / (double)totalFaults * 100) : 0;

                    sheet.Cells[currentRow, 1].Value = status;
                    sheet.Cells[currentRow, 2].Value = count;
                    sheet.Cells[currentRow, 3].Value = percentage / 100; // Convert to decimal for percentage format
                    sheet.Cells[currentRow, 3].Style.Numberformat.Format = "0.0%";
                    currentRow++;
                }
                currentRow += 2;
            }

            // Complete Priority Distribution (All Priorities)
            if (report.AllPrioritiesWithCounts.Any())
            {
                sheet.Cells[currentRow, 1].Value = "Fault Distribution by Priorities";
                sheet.Cells[currentRow, 1].Style.Font.Size = 14;
                sheet.Cells[currentRow, 1].Style.Font.Bold = true;
                currentRow++;

                sheet.Cells[currentRow, 1].Value = "Priority";
                sheet.Cells[currentRow, 2].Value = "Count";
                sheet.Cells[currentRow, 3].Value = "Percentage";
                sheet.Cells[currentRow, 1, currentRow, 3].Style.Font.Bold = true;
                sheet.Cells[currentRow, 1, currentRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[currentRow, 1, currentRow, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                currentRow++;

                var totalFaults = report.Performance.TotalFaults;
                var priorityOrder = new List<string> { "Critical", "High", "Medium", "Low" };

                foreach (var priority in priorityOrder)
                {
                    var count = report.AllPrioritiesWithCounts.ContainsKey(priority) ? report.AllPrioritiesWithCounts[priority] : 0;
                    var percentage = totalFaults > 0 ? (count / (double)totalFaults * 100) : 0;

                    sheet.Cells[currentRow, 1].Value = priority;
                    sheet.Cells[currentRow, 2].Value = count;
                    sheet.Cells[currentRow, 3].Value = percentage / 100; // Convert to decimal for percentage format
                    sheet.Cells[currentRow, 3].Style.Numberformat.Format = "0.0%";
                    currentRow++;
                }
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            // Add borders to the tables
            var statusTableRange = sheet.Cells[2, 1, 2 + 5, 3]; // 5 statuses + header
            statusTableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            statusTableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            statusTableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            statusTableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

            var priorityStartRow = 2 + 5 + 2 + 1; // After status table with gap
            var priorityTableRange = sheet.Cells[priorityStartRow, 1, priorityStartRow + 4, 3]; // 4 priorities + header
            priorityTableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            priorityTableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            priorityTableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            priorityTableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        private void GenerateTrendsSheet(ExcelWorksheet sheet, TechnicianReportViewModel report)
        {
            sheet.Cells[1, 1].Value = "Monthly Performance Trends";
            sheet.Cells[1, 1].Style.Font.Size = 14;
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1, 1, 5].Merge = true;

            // Headers
            var headers = new[] { "Month", "Total Faults", "Completed Faults", "Completion Rate", "Avg Resolution Days" };
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[3, i + 1].Value = headers[i];
                sheet.Cells[3, i + 1].Style.Font.Bold = true;
                sheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            }

            // Data
            for (int i = 0; i < report.MonthlyTrends.Count; i++)
            {
                var trend = report.MonthlyTrends[i];
                sheet.Cells[i + 4, 1].Value = trend.Month;
                sheet.Cells[i + 4, 2].Value = trend.TotalFaults;
                sheet.Cells[i + 4, 3].Value = trend.CompletedFaults;
                sheet.Cells[i + 4, 4].Value = trend.CompletionRate / 100; // Convert to decimal for percentage format
                sheet.Cells[i + 4, 4].Style.Numberformat.Format = "0.0%";

                if (trend.AverageResolutionDays > 0)
                {
                    sheet.Cells[i + 4, 5].Value = Math.Round(trend.AverageResolutionDays, 2);
                    sheet.Cells[i + 4, 5].Style.Numberformat.Format = "0.00";
                }
                else
                {
                    sheet.Cells[i + 4, 5].Value = "-";
                    sheet.Cells[i + 4, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }
    }
}
