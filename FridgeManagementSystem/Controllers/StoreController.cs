using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.ViewModels;

namespace FridgeManagementSystem.Controllers
{
    public class StoreController : Controller
    {
        private readonly FridgeManagementSystemContext _context;

        public StoreController(FridgeManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(StoreSearchViewModel searchModel, int pageIndex = 1)
        {
            
            var query = _context.Fridges
                .Where(f => f.IsActive && f.Status == "Available")
                .Include(f => f.FridgeType) as IQueryable<Fridge>;

            // Apply filters
            if (!string.IsNullOrEmpty(searchModel.Brand))
            {
                query = query.Where(f => f.FridgeType.Brand == searchModel.Brand);
            }

            if (!string.IsNullOrEmpty(searchModel.Category))
            {
                query = query.Where(f => f.Category == searchModel.Category);
            }

            if (!string.IsNullOrEmpty(searchModel.Search))
            {
                query = query.Where(f =>
                    f.FridgeType.Name.Contains(searchModel.Search) ||
                    f.FridgeType.Brand.Contains(searchModel.Search) ||
                    f.FridgeType.Model.Contains(searchModel.Search));
            }

            // Apply sorting
            switch (searchModel.Sort)
            {
                case "price_asc":
                    query = query.OrderBy(f => f.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(f => f.Price);
                    break;
                default:
                    query = query.OrderByDescending(f => f.CreatedAt);
                    break;
            }

            // Pagination
            var pageSize = 12;
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var fridges = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Fridges = fridges;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View(searchModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var fridge = await _context.Fridges
                .Include(f => f.FridgeType)
                .Include(f => f.Supplier)
                .FirstOrDefaultAsync(f => f.FridgeId == id && f.IsActive && f.Status == "Available");

            if (fridge == null)
                return NotFound();

            return View(fridge);
        }
    }
}