namespace FridgeManagementSystem.ViewModels
{
    public class PriorityMetrics
    {
        public int Priority { get; set; } // 1=Low, 2=Medium, 3=High, 4=Critical
        public int Attended { get; set; }
        public int Completed { get; set; }
        public double SuccessRate { get; set; }
        public double AvgCompletionTime { get; set; }

    }
}