namespace FridgeManagementSystem.ViewModels
{
    public class FaultViewModel
    {
        public List<Fault> Faults { get; set; }
        public int ReportedFaultsCount { get; set; }
       
        public int UrgentFaultsCount { get; set; }
        public int InProgressFaultsCount { get; set; }
        public int CompletedFaultsCount { get; set; }
        
    }
}
