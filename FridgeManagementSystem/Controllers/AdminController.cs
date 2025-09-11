using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FridgeManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
       
        public AdminController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            
        }
        //private readonly FridgeManagementSystemContext _context;

        //public AdminController(FridgeManagementSystemContext context)
        //{
        //    _context = context;
        //}
        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users.ToList();
            return View(users);
           
           

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

        //}
        //public async Task<IActionResult> DeleteUser(int id)
        //{
        //    var user = await userManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
        //        return View("NotFound");
        //    }
        //    else
        //    {
        //        var result = await userManager.DeleteAsync(user);
        //        if (result.Succeeded)
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }
        //    }
        //}
        public IActionResult Index()
        {
            return View();
        }
    }
}
