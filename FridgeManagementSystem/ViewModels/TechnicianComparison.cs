namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianComparison
    {
        public string? TechnicianName { get; set; }
        public int CompletedFaults { get; set; }
        public double AvgCompletionTime { get; set; }
        public double CompletionRate { get; set; }
        
        public int Rank { get; set; }
    }
}