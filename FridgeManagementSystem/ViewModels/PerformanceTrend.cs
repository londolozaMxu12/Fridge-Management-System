namespace FridgeManagementSystem.ViewModels
{
    public class PerformanceTrend
    {
        public string Period { get; set; }
        public double CompletionRate { get; set; }
        
        public int TotalFaults { get; set; }
        public int CompletedFaults { get; set; }
    }
}