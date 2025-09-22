// Services/AdminSeedService.cs
using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

public interface IAdminSeedService
{
    Task SeedAdminUserAsync();
}

public class AdminSeedService : IAdminSeedService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeedService> _logger;

    public AdminSeedService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        ILogger<AdminSeedService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAdminUserAsync()
    {
        try
        {
            // Check if Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                Console.WriteLine("Admin role created successfully.");

            }

            // Get admin user configuration from appsettings.json
            var adminEmail = _configuration["AdminUser:Email"] ?? "admin@123.com";
            var adminPassword = _configuration["AdminUser:Password"] ?? "Admin@123";
            var adminFullName = _configuration["AdminUser:FullName"] ?? "System Administrator";

            // Check if admin user already exists
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Create admin user
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    //EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // Add admin user to Admin role
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    //Console.WriteLine("Admin user created successfully with email: {Email}", adminEmail);
                }
                else
                {
                   // Console.WriteLine("Failed to create admin user. Errors: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                // Ensure existing admin user has the Admin role
                if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    //Console.WriteLine("Added Admin role to existing user: {Email}", adminEmail);
                }
            }
        }
        catch (Exception)
        {
            //Console.WriteLine("An error occurred while seeding admin user.", ex);
            throw;
        }
    }
}