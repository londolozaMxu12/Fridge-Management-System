using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class ActivityLogViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Action")]
        public string Action { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Entity Type")]
        public string EntityType { get; set; }

        [Display(Name = "Entity ID")]
        public int? EntityId { get; set; }

        [Display(Name = "User")]
        public string UserName { get; set; }

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; }

        [Display(Name = "IP Address")]
        public string IPAddress { get; set; }

        // Computed properties for display
        [Display(Name = "Formatted Time")]
        public string FormattedTimestamp => Timestamp.ToString("MMM dd, yyyy HH:mm");

        [Display(Name = "Time Ago")]
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - Timestamp;

                if (timeSpan <= TimeSpan.FromSeconds(60))
                    return $"{timeSpan.Seconds} seconds ago";
                else if (timeSpan <= TimeSpan.FromMinutes(60))
                    return $"{timeSpan.Minutes} minutes ago";
                else if (timeSpan <= TimeSpan.FromHours(24))
                    return $"{timeSpan.Hours} hours ago";
                else if (timeSpan <= TimeSpan.FromDays(30))
                    return $"{timeSpan.Days} days ago";
                else
                    return Timestamp.ToString("MMM dd, yyyy");
            }
        }

        public string ActionBadgeClass => Action?.ToLower() switch
        {
            "created" => "badge-success",
            "updated" => "badge-warning",
            "deleted" => "badge-danger",
            "approved" => "badge-primary",
            "rejected" => "badge-danger",
            "submitted" => "badge-info",
            "converted" => "badge-success",
            _ => "badge-secondary"
        };
    }
}
