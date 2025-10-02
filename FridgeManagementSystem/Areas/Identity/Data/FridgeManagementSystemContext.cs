using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
    //public DbSet<ApplicationUser> Admin {  get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeType> EmployeeTypes { get; set; }
    public DbSet<Notification> Notifications { get; set; }
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
    public DbSet<CartDetails> CartDetails { get; set; }
    
    public DbSet<FridgeType> FridgeType { get; set; }
    public DbSet<OrderStatus> OrderStatus { get; set; }
    public DbSet<ShoppingCart> ShoppingCart { get; set; }
    public DbSet<PurchasingOrderDetails> PurchasingOrderDetails { get; set; }
    public DbSet<PurchasingOrder> PurchasingOrders { get; set; }
    public DbSet<Quotation> Quotations { get; set; }
    public DbSet<StockLevel> StockLevels { get; set; }
    public DbSet<Suburb> Suburbs { get; set; }
    
    public DbSet<FaultReport> FaultReports { get; set; }
    public DbSet<Allocation> Allocations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        // Configure relationships
        // Configure the self-referencing foreign key with NO ACTION
        //builder.Entity<ApplicationUser>()
        //    .HasOne(u => u.ApprovedBy)
        //    .WithMany()
        //    .HasForeignKey(u => u.ApprovedById)
        //    .OnDelete(DeleteBehavior.NoAction);

        //builder.Entity<MaintenanceRecord>()
        //.HasOne(m => m.MaintenanceTechnician)
        //.WithMany()
        //.HasForeignKey(m => m.MaintenanceTechnicianId)
        //.OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PurchasingOrderDetails>()
        .Property(p => p.UnitPrice)
        .HasPrecision(18, 4);

        // Customer relationship 
        builder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithOne(u => u.Customers)
            .HasForeignKey<Customer>(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Customer>()
        .HasOne(c => c.CreatedBy)
        .WithMany()
        .HasForeignKey(c => c.CreatedById)
        .OnDelete(DeleteBehavior.NoAction);

        // Employee relationships
        builder.Entity<Employee>()
            .HasOne(e => e.User)
            .WithOne(u => u.Employees)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Employee>()
            .HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Employee>()
            .HasOne(e => e.EmployeeType)
            .WithMany()
            .HasForeignKey(e => e.EmployeeTypeId)
            .OnDelete(DeleteBehavior.NoAction);

        // Supplier relationships
        builder.Entity<Supplier>()
            .HasOne(s => s.User)
            .WithOne(u => u.Suppliers)
            .HasForeignKey<Supplier>(s => s.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Supplier>()
            .HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedById)
            .OnDelete(DeleteBehavior.NoAction);

        // Employee number configuration
        builder.Entity<Employee>()
            .Property(e => e.EmployeeNo)
            .IsRequired()
            .HasMaxLength(20);



    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await GenerateEmployeeNumbers();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        GenerateEmployeeNumbers().Wait();
        return base.SaveChanges();
    }

    private async Task GenerateEmployeeNumbers()
    {
        var employeeNumberService = new EmployeeNumberService(this);

        var newEmployees = ChangeTracker.Entries<Employee>()
            .Where(e => e.State == EntityState.Added && string.IsNullOrEmpty(e.Entity.EmployeeNo))
            .Select(e => e.Entity)
            .ToList();

        foreach (var employee in newEmployees)
        {
            employee.EmployeeNo = await employeeNumberService.GenerateEmployeeNumberAsync();
        }
    }
}
