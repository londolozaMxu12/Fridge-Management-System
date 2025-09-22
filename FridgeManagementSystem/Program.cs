using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Areas.Identity.Pages.Account.Manage;
using FridgeManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
//using FridgeManagementSystem.Managers.Validators;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("FridgeManagementSystemContextConnection") ?? throw new InvalidOperationException("Connection string 'FridgeManagementSystemContextConnection' not found.");

builder.Services.AddDbContext<FridgeManagementSystemContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;

    })
    .AddRoles<IdentityRole>().AddEntityFrameworkStores<FridgeManagementSystemContext>()
    .AddDefaultTokenProviders();
    //.AddUserValidator<ActiveUserValidator<ApplicationUser>>();

// Register Admin Seed Service
builder.Services.AddScoped<IAdminSeedService, AdminSeedService>();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IEmailSender, DummyEmailSender>();
builder.Services.AddRazorPages();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";           
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Ensure 'Admin' role exists at application startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var adminRole = await roleManager.FindByNameAsync("Admin");
    if (adminRole == null)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
}

var cultureInfo = new CultureInfo("en-ZA");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed the admin user and role
//using (var scope = app.Services.CreateScope())
//{
//    var seedService = scope.ServiceProvider.GetRequiredService<IAdminSeedService>();
//    await seedService.SeedAdminUserAsync();
//}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var adminSeedService = services.GetRequiredService<IAdminSeedService>();
        await adminSeedService.SeedAdminUserAsync();

        //Console.WriteLine("Admin user seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        //Console.WriteLine("An error occurred while seeding admin user.", ex);
    }
}
app.Run();
