using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class FridgeController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly IWebHostEnvironment _environment;

        public FridgeController(FridgeManagementSystemContext context, IWebHostEnvironment environment)
        {
            _context = context;
           _environment = environment;
        }
        public IActionResult Index()
        {
            var fridges= _context.Fridges.OrderByDescending(f => f.FridgeId).ToList();
            return View(fridges);
        }
        public IActionResult Create()
        { 
            return View();
        }
        [HttpPost]
        public IActionResult Create(FridgeViewModel fridgeViewModel)
        {
            if (fridgeViewModel.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }
            if (!ModelState.IsValid)
            {
                return View(fridgeViewModel);
            }

            // save the image file

            // 1️⃣ Generate unique filename
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff")
                                 + Path.GetExtension(fridgeViewModel.ImageFile!.FileName);

            // 2️⃣ Full path to save on server (wwwroot)
            string imageFullPath = Path.Combine(_environment.WebRootPath, "Images", "Fridges", newFileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(imageFullPath)!);

            // 3️⃣ Save the file
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                fridgeViewModel.ImageFile.CopyToAsync(stream);
            }

            // save the new fridge in the database

            Fridge fridge = new Fridge()
            {
                SerialNumber = fridgeViewModel.SerialNumber,
                Name = fridgeViewModel.Name,
                Brand = fridgeViewModel.Brand,
                Model = fridgeViewModel.Model,
                Description = fridgeViewModel.Description,
                Price = fridgeViewModel.Price,
                ImageFile = newFileName,
                CreatedAt = DateTime.Now,
            };

            _context.Fridges.Add(fridge);
            _context.SaveChanges();
            return RedirectToAction("Index", "Fridge");
        }
       
    }
}
