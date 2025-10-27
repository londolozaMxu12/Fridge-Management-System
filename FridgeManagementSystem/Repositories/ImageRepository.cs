using Microsoft.Extensions.Options;

namespace FridgeManagementSystem.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly ImageSettings _settings;
        private readonly IWebHostEnvironment _environment;

        public ImageRepository(IOptions<ImageSettings> settings, IWebHostEnvironment environment)
        {
            _settings = settings.Value;
            _environment = environment;
        }

        public string GetWebImagePath(string imageFileName)
        {
            if (string.IsNullOrEmpty(imageFileName))
                return "/Images/placeholder.jpg";

            return $"{_settings.WebImagePath}{imageFileName}";
        }

        public string GetPhysicalImagePath(string imageFileName)
        {
            if (string.IsNullOrEmpty(imageFileName))
                return null;

            var contentRootPath = _environment.ContentRootPath;
            return Path.Combine(contentRootPath, _settings.ImageStoragePath, imageFileName);
        }

        public bool ImageExists(string imageFileName)
        {
            if (string.IsNullOrEmpty(imageFileName))
                return false;

            var physicalPath = GetPhysicalImagePath(imageFileName);
            return File.Exists(physicalPath);
        }

        public async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Validate file size
            if (imageFile.Length > _settings.MaxFileSize)
                throw new InvalidOperationException($"File size exceeds the maximum allowed size of {_settings.MaxFileSize} bytes.");

            // Validate file extension
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!_settings.AllowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException($"File extension {fileExtension} is not allowed.");

            // Generate unique filename
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);

            // Get physical path
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, _settings.ImageStoragePath);

            // Ensure directory exists
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }
    }
}
