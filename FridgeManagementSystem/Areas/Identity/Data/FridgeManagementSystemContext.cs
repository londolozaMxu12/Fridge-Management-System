using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Data;

public class FridgeManagementSystemContext : IdentityDbContext<ApplicationUser>
{
    public FridgeManagementSystemContext(DbContextOptions<FridgeManagementSystemContext> options)
        : base(options)
    {
  
    }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<IdentityRole> IdentityRoles { get; set; }

    // FOR BUSINESS TABLES
    public DbSet<ApplicationUser> Admin {  get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Fridge> Fridges { get; set; }
    public DbSet<Fault> Faults { get; set; }
    public DbSet<FaultTechnician> FaultTechnicians { get; set; }
    public DbSet<RepairSchedule> RepairSchedules { get; set; }
    public DbSet<FridgeRequest> FridgeRequests { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<CustomerData> CustomerDatas { get; set; }
    public DbSet<CustomerLiaison> CustomerLiaisons { get; set; }
    public DbSet<FridgeInventory> FridgeInventories { get; set; }
    public DbSet<InventoryLiaison> InventoryLiaisons { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    public DbSet<MaintenanceTech> MaintenanceTechs { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    public DbSet<PurchasingManager> PurchasingManagers { get; set; }
    public DbSet<PurchasingOrderDetails> OrderDetails { get; set; }
    public DbSet<PurchasingOrder> PurchasingOrders { get; set; }
    public DbSet<Quotation> Quotations { get; set; }
    public DbSet<StockLevel> StockLevels { get; set; }
    public DbSet<Suburb> Suburbs { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<FaultReport> FaultReports { get; set; }
    public DbSet<Allocation> Allocations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        
    }
}
