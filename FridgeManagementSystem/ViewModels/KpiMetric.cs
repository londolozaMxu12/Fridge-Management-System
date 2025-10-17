namespace FridgeManagementSystem.ViewModels
{
    public class KpiMetric
    {
        public string Name { get; set; }
        public double CurrentValue { get; set; }
        public double TargetValue { get; set; }
        public double PreviousValue { get; set; }
        public string Status { get; set; } // Improving, Declining, Stable
        public double Variance { get; set; }
    }
}