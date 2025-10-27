namespace FridgeManagementSystem.Repositories
{
    public interface IImageRepository
    {
        string GetWebImagePath(string imageFileName);
        string GetPhysicalImagePath(string imageFileName);
        Task<string> SaveImageAsync(IFormFile imageFile);
        bool ImageExists(string imageFileName);
    }

}
