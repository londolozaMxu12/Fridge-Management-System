using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Profile/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                ContactNo = user.ContactNo,
                Address = user.Address,
                City = user.City,
                Suburb = user.Suburb,
                PostalCode = user.PostalCode,
                CreatedAt = user.CreatedAt,
                ApprovalStatus = user.ApprovalStatus
            };

            return View(model);
        }

        // GET: /Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                ContactNo = user.ContactNo,
                Address = user.Address,
                City = user.City,
                Suburb = user.Suburb,
                PostalCode = user.PostalCode,
                CreatedAt = user.CreatedAt,
                ApprovalStatus = user.ApprovalStatus
            };

            return View(model);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Update user properties
            user.FullName = model.FullName;
            user.ContactNo = model.ContactNo;
            user.Address = model.Address;
            user.City = model.City;
            user.Suburb = model.Suburb;
            user.PostalCode = model.PostalCode;

            // Update email if changed
            if (model.Email != user.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Your profile has been updated successfully.";
                return RedirectToAction(nameof(MyProfile));
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Profile/DiscardChanges
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DiscardChanges()
        {
            return RedirectToAction(nameof(MyProfile));
        }
    }
}
