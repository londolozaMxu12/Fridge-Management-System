using FridgeManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FridgeManagementSystem.Controllers
{
    //[Authorize(Roles = "Administrator")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole role)
        {
            if (ModelState.IsValid)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(role.Name.Trim()));
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Role '{role.Name}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(role);
            //if (!_roleManager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
            //{
            //    _roleManager.CreateAsync(new IdentityRole(role.Name)).GetAwaiter().GetResult();
            //}
            //return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        
         // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                // Check if role has users
                var context = HttpContext.RequestServices.GetService<FridgeManagementSystemContext>();
                var usersInRole = await context.UserRoles.AnyAsync(ur => ur.RoleId == id);
                
                if (usersInRole)
                {
                    TempData["ErrorMessage"] = $"Cannot delete role '{role.Name}' because it has users assigned to it.";
                    return View(role);
                }

                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Role '{role.Name}' deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(await _roleManager.FindByIdAsync(id));
        }
    }
    
}
