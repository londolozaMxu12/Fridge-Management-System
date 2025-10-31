using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Areas.Identity.Pages.Account.Manage;
using FridgeManagementSystem.Authorization;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using FridgeManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using OfficeOpenXml;
using QuestPDF.Infrastructure;
using System.Globalization;
//using FridgeManagementSystem.Managers.Validators;

var builder = WebApplication.CreateBuilder(args);
// Set QuestPDF license (Community version - free for non-commercial use)
QuestPDF.Settings.License = LicenseType.Community;

// Set EPPlus license context (EPPlus is free for non-commercial use)
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerLiaisonAccess", policy =>
        policy.RequireAssertion(context =>
        {
            // Admin always has access
            if (context.User.IsInRole("Admin"))
                return true;

            // Employees will be checked by the custom handler
            if (context.User.IsInRole("Employee"))
                return true;

            return false;
        }));
});

builder.Services.Configure<ImageSettings>(
builder.Configuration.GetSection("ImageSettings"));
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IAuthorizationHandler, CustomerLiaisonAuthorizationHandler>();
builder.Services.AddScoped<IOrderNotificationRepository, OrderNotificationRepository>();
builder.Services.AddScoped<IFaultNotificationRepository, FaultNotificationRepository>();
builder.Services.AddScoped<ITechnicianReportRepository, TechnicianReportRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IEmployeeNumberService, EmployeeNumberService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();

// Register Admin Seed Service
builder.Services.AddScoped<IAdminSeedService, AdminSeedService>();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IEmailSender, DummyEmailSender>();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddTransient<IHomeRepository, HomeRepository>();
//builder.Services.AddTransient<ICartRepository, CartRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
    options.SuppressXFrameOptionsHeader = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";           
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var employeeNumberService = scope.ServiceProvider.GetRequiredService<IEmployeeNumberService>();
    
}

// Ensure 'Admin' role exists at application startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var adminRole = await roleManager.FindByNameAsync("Admin");
    if (adminRole == null)
    {
         
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // ALSO ENSURE 'Employee' ROLE EXISTS
    var employeeRole = await roleManager.FindByNameAsync("Employee");
    if (employeeRole == null)
    {
        await roleManager.CreateAsync(new IdentityRole("Employee"));
    }
}

var cultureInfo = new CultureInfo("en-ZA");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot/Images")),
    RequestPath = "/Images"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


// Add this for API error handling
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        // Handle API 404s differently than page 404s
        if (context.Request.Path.StartsWithSegments("/api") ||
            context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"success\":false,\"message\":\"Resource not found\"}");
        }
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var adminSeedService = services.GetRequiredService<IAdminSeedService>();
        await adminSeedService.SeedAdminUserAsync();

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        
    }
}
app.Run();
