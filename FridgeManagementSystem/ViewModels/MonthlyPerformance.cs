namespace FridgeManagementSystem.ViewModels
{
    public class MonthlyPerformance
    {
        public string? Month { get; set; }
        public int CompletedFaults { get; set; }
        public int TotalFaults { get; set; }
        public double AverageCompletionTime { get; set; } = 0;
    }
}