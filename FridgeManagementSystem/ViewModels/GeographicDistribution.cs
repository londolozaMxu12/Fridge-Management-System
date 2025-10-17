namespace FridgeManagementSystem.ViewModels
{
    public class GeographicDistribution
    {
        public string City { get; set; }
        public int TotalFaults { get; set; }
        public int CriticalFaults { get; set; }
        public double ResolutionRate { get; set; }
        
        public string HotspotLevel { get; set; } // Low, Medium, High, Critical
    }
}