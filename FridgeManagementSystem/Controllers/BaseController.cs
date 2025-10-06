using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FridgeManagementSystem.Areas.Identity.Data;

public class BaseController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public BaseController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    protected async Task<string> GetLayoutAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return "_Layout";

        var roles = await _userManager.GetRolesAsync(user);

        return roles.FirstOrDefault()?.ToLower() switch
        {
            "admin" => "_AdminLayout",
            //"employee" => "_EmployeeLayout",
            //"supplier" => "_SupplierLayout",
            "customer" => "_CustomerLayout",
            _ => "_Layout"
        };
    }
}