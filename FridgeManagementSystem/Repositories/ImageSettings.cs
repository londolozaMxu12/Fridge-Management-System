namespace FridgeManagementSystem.Repositories
{
    public class ImageSettings
    {
        public string ImageStoragePath { get; set; } = "wwwroot/Images/Fridges";
        public string WebImagePath { get; set; } = "/Images/Fridges/";
        public long MaxFileSize { get; set; } = 5242880;
        public string[] AllowedExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    }
}
