using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Data;

public class FridgeManagementSystemContext : IdentityDbContext<IdentityUser>
{
    public FridgeManagementSystemContext(DbContextOptions<FridgeManagementSystemContext> options)
        : base(options)
    {
  
    }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<IdentityRole> IdentityRoles { get; set; }

    // FOR BUSINESS TABLES
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Fridge> Fridges { get; set; }
    public DbSet<Fault> Faults { get; set; }
    public DbSet<FaultTechnician> FaultTechnicians { get; set; }
    public DbSet<RepairSchedule> RepairSchedules { get; set; }
    public DbSet<FridgeRequest> FridgeRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
