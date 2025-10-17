using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis;

namespace FridgeManagementSystem.ViewModels
{
    public class FaultAnalysisViewModel
    {
        public DateTime? ReportDateFrom { get; set; }
        public DateTime? ReportDateTo { get; set; }
        public string AnalysisType { get; set; } = "comprehensive";

        // Executive Summary
        public ExecutiveSummary Summary { get; set; } = new();

        // Trend Analysis
        public List<FaultTrend> Trends { get; set; } = new();
        
        // Geographic Analysisnalysis Report
        public List<GeographicDistribution> GeographicData { get; set; } = new();

        // KPI Metrics
        public List<KpiMetric> Kpis { get; set; } = new();

        // Export Properties
        public string ReportTitle => "Comprehensive Fault Analysis Report";
        public string GeneratedOn => DateTime.Now.ToString("dd-MM-yyyy HH:mm");
        public string ReportPeriod => $"{ReportDateFrom?.ToString("dd-MM-yyyy")} to {ReportDateTo?.ToString("dd-MM-yyyy")}";
    }
}
