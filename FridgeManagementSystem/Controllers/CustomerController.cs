using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FridgeManagementSystemContext _context;
        public CustomerController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, FridgeManagementSystemContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;

        }
        public IActionResult index()
        {
            return View();
        }

        public async Task<IActionResult> MyFridges()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var fridges = await _context.Fridges
                //.Include(f => f.Location)
                .Include(f => f.Supplier)
                .Where(f => f.Customer.UserId == userId && f.IsActive && f.Status == "Assigned")
                .ToListAsync();

            return View(fridges);
        }
        // GET: Customer/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var customer = await _context.Customers
                .Include(c => c.User)
                //.Include(c => c.Location)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
        public async Task<IActionResult> ListCustomers()
        {

            //var user = userManager.Users.ToList();
            var customer = await _userManager.GetUsersInRoleAsync("Customer");
           
            return View(customer);
        }

        public IActionResult AllocatedFridge()
        {
            return View();
        }


        //public async Task<IActionResult> ListUsers()
        //{
        //    var users = userManager.Users.ToList();
        //    var userRoles = new List<object>();
        //    foreach (var user in users)
        //    {
        //        var roles = await userManager.GetRolesAsync(user);
        //        userRoles.Add(new
        //        {
        //            user.UserName,
        //            user.Email,
        //            Roles = roles
        //        });
        //    }

        //    return Json(userRoles);
        //public CustomerController(FridgeManagementSystemContext db) { _db = db; }

        //public async Task<IActionResult> Index() => View(await _db.Customers.Include(c => c.Allocations).ToListAsync());
        ////public IActionResult Index()
        ////{
        ////    return View();
        ////}
        //public IActionResult Create() => View();

        //[HttpPost]
        //public async Task<IActionResult> Create(Customer customer)
        //{
        //    if (!ModelState.IsValid) return View(customer);
        //    _db.Add(customer);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var customer = await _db.Customers.FindAsync(id);
        //    if (customer == null) return NotFound();
        //    return View(customer);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit(Customer customer)
        //{
        //    if (!ModelState.IsValid) return View(customer);
        //    _db.Update(customer);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var customer = await _db.Customers.FindAsync(id);
        //    if (customer == null) return NotFound();
        //    _db.Customers.Remove(customer);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        //public async Task<IActionResult> Details(int id)
        //{
        //    var customer = await _db.Customers
        //        .Include(c => c.Allocations)
        //        .ThenInclude(a => a.Fridge)
        //        .FirstOrDefaultAsync(c => c.CustomerId == id);
        //    if (customer == null) return NotFound();
        //    return View(customer);
        //}
    }
}
