namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianEfficiency
    {
        public string? TechnicianName { get; set; }
        public int CompletedFaults { get; set; }
        public double EfficiencyScore { get; set; }
        
        public string? PerformanceTier { get; set; } // Beginner, Intermediate, Expert, Master
    }
}