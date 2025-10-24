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
    public DbSet<ScheduleMaintenance> ScheduleMaintenances { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. Configure ApplicationUser relationships
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Customers)
            .WithOne(c => c.User)
            .HasForeignKey<Customer>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Employees)
            .WithOne(e => e.User)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Suppliers)
            .WithOne(s => s.User)
            .HasForeignKey<Supplier>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.ApprovedBy)
            .WithMany()
            .HasForeignKey(u => u.ApprovedById)
            .OnDelete(DeleteBehavior.SetNull);

        // 2. Configure Allocation relationships
        builder.Entity<Allocation>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Allocations)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Allocation>()
            .HasOne(a => a.AllocatedBy)
            .WithMany()
            .HasForeignKey(a => a.AllocatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Allocation>()
            .HasOne(a => a.Fridge)
            .WithMany(f => f.Allocations)
            .HasForeignKey(a => a.FridgeId)
            .OnDelete(DeleteBehavior.Restrict);

        //3.Configure Fridge relationships
        // TEMPORARILY COMMENT OUT UNIQUE CONSTRAINT:
        builder.Entity<Fridge>()
            .HasIndex(f => f.SerialNumber)
            .IsUnique();

        builder.Entity<Fridge>()
            .Property(f => f.Price)
            .HasPrecision(16, 2);

        builder.Entity<Fridge>()
            .HasOne(f => f.Customer)
            .WithMany(c => c.Fridges)
            .HasForeignKey(f => f.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Fridge>()
            .HasOne(f => f.Supplier)
            .WithMany(s => s.Fridges)
            .HasForeignKey(f => f.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Fridge>()
            .HasOne(f => f.FridgeType)
            .WithMany(ft => ft.Fridges)
            .HasForeignKey(f => f.FridgeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Fridge>()
            .HasOne(f => f.CreatedBy)
            .WithMany()
            .HasForeignKey(f => f.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // 4. Configure Order relationships
        builder.Entity<Order>()
            .Property(o => o.ShippingFee)
            .HasPrecision(16, 2);

        builder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(o => o.Fridge)
            .WithOne(f => f.Order)
            .HasForeignKey<Order>(o => o.FridgeId)
            .OnDelete(DeleteBehavior.SetNull);

        // 5. Configure OrderItem relationships
        builder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice)
            .HasPrecision(16, 2);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Fridge)
            .WithMany(f => f.OrderItems)
            .HasForeignKey(oi => oi.FridgeId)
            .OnDelete(DeleteBehavior.Restrict);

        // 6. Configure ShoppingCart relationships
        builder.Entity<ShoppingCart>()
            .HasOne(sc => sc.User)
            .WithOne(u => u.ShoppingCart)
            .HasForeignKey<ShoppingCart>(sc => sc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 7. Configure CartItem relationships
        builder.Entity<CartItem>()
            .HasOne(ci => ci.ShoppingCart)
            .WithMany(sc => sc.Items)
            .HasForeignKey(ci => ci.ShoppingCartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CartItem>()
            .HasOne(ci => ci.Fridge)
            .WithMany(f => f.CartItems)
            .HasForeignKey(ci => ci.FridgeId)
            .OnDelete(DeleteBehavior.Cascade);

        // 8. Configure Employee relationships
        builder.Entity<Employee>()
            .HasIndex(e => e.EmployeeNo)
            .IsUnique();

        builder.Entity<Employee>()
            .HasOne(e => e.EmployeeType)
            .WithMany()
            .HasForeignKey(e => e.EmployeeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Employee>()
            .HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // 9. Configure Customer relationships
        builder.Entity<Customer>()
            .HasOne(c => c.CreatedBy)
            .WithMany()
            .HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // 10. Configure Supplier relationships
        builder.Entity<Supplier>()
            .HasOne(s => s.CreatedBy)
            .WithMany()
            .HasForeignKey(s => s.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // 11. Configure Notification relationships
        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 12. Add missing decimal precision configurations
        builder.Entity<PurchasingOrderDetails>()
            .Property(p => p.UnitPrice)
            .HasPrecision(18, 4);

        builder.Entity<RepairSchedule>()
            .Property(r => r.EstimatedHours)
            .HasPrecision(5, 2);

        // ScheduleMaintenance configuration
        builder.Entity<ScheduleMaintenance>(entity =>
        {
            entity.HasKey(e => e.scheduleMaintenanceId);


            entity.Property(e => e.Description)
                .HasMaxLength(500);


            // Relationship with MaintenanceTechnician (Employee)
            entity.HasOne(e => e.MaintenanceTechnician)
                .WithMany(e => e.ScheduledMaintenances)
                .HasForeignKey(e => e.MaintenanceTechnicianId)
                .OnDelete(DeleteBehavior.NoAction);


        });
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
