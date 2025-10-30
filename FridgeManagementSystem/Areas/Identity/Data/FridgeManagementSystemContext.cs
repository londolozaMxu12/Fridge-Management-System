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

    // Remove duplicate - ApplicationUsers is already inherited from IdentityDbContext
    // public DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public DbSet<IdentityRole> IdentityRoles { get; set; }

    // FOR BUSINESS TABLES
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
    //public DbSet<City> Cities { get; set; }
    public DbSet<CustomerData> CustomerDatas { get; set; }
    public DbSet<CustomerLiaison> CustomerLiaisons { get; set; }
    public DbSet<FridgeInventory> FridgeInventories { get; set; }
    public DbSet<InventoryLiaison> InventoryLiaisons { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    public DbSet<MaintenanceTech> MaintenanceTechs { get; set; }
    //public DbSet<Province> Provinces { get; set; }
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    public DbSet<PurchasingManager> PurchasingManagers { get; set; }
    public DbSet<CartDetails> CartDetails { get; set; }
    public DbSet<FridgeType> FridgeType { get; set; }
    public DbSet<OrderStatus> OrderStatus { get; set; }
    public DbSet<ShoppingCart> ShoppingCart { get; set; }
    public DbSet<PurchasingOrderDetails> PurchasingOrderDetails { get; set; }
    public DbSet<PurchasingOrder> PurchasingOrders { get; set; }
    public DbSet<Quotation> Quotations { get; set; }
    public DbSet<StockLevel> StockLevels { get; set; }
    //public DbSet<Suburb> Suburbs { get; set; }
    public DbSet<FaultReport> FaultReports { get; set; }
    public DbSet<Allocation> Allocations { get; set; }
    public DbSet<ScheduleMaintenance> ScheduleMaintenances { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<RFQSupplier> RFQSuppliers { get; set; }
    public DbSet<DeliveryNote> DeliveryNotes { get; set; }

    public DbSet<RFQ> RFQs { get; set; }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

       
        // Invoice configuration (if not already present)
        builder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.OrderId);

            // This should already match the configuration above
            entity.HasOne(i => i.Order)
                  .WithMany(o => o.Invoices)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(i => i.Subtotal)
                  .HasPrecision(16, 2);

            entity.Property(i => i.ShippingFee)
                  .HasPrecision(16, 2);

            entity.Property(i => i.TotalAmount)
                  .HasPrecision(16, 2);
        });

        // InvoiceItem configuration (if not already present)
        builder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(ii => ii.Invoice)
                  .WithMany(i => i.InvoiceItems)
                  .HasForeignKey(ii => ii.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ii => ii.Fridge)
                  .WithMany()
                  .HasForeignKey(ii => ii.FridgeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(ii => ii.UnitPrice)
                  .HasPrecision(16, 2);

            entity.Property(ii => ii.TotalPrice)
                  .HasPrecision(16, 2);
        });

        // 1. Configure ApplicationUser relationships - FIXED
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Customers)
            .WithOne(c => c.User)
            .HasForeignKey<Customer>(c => c.Id)  // Use Id as FK since UserId was removed
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

        // Add this configuration for Quotation decimal properties
        builder.Entity<Quotation>(entity =>
        {
            entity.Property(q => q.ShippingCost)
                .HasPrecision(18, 2);  // Or use decimal(16,2) if you prefer

            entity.Property(q => q.TotalPrice)
                .HasPrecision(18, 2);

            entity.Property(q => q.UnitPrice)
                .HasPrecision(18, 2);
        });

        // 2. Configure Allocation relationships - ONLY ONCE
        builder.Entity<Allocation>(entity =>
        {
            entity.HasKey(a => a.AllocationId);

            entity.Property(e => e.FridgeId)
             .HasColumnName("FridgeId");

            // Customer relationship
            entity.HasOne(a => a.Customer)
                .WithMany(c => c.Allocations)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fridge relationship - ONLY ONE relationship to Fridge
            entity.HasOne(a => a.Fridge)
                .WithMany(f => f.Allocations)
                .HasForeignKey(a => a.FridgeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order relationship
            entity.HasOne(a => a.Order)
                .WithMany(o => o.Allocations)
                .HasForeignKey(a => a.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // AllocatedBy relationship
            entity.HasOne(a => a.AllocatedBy)
                .WithMany()
                .HasForeignKey(a => a.AllocatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // IGNORE all problematic FridgeId properties
            entity.Ignore("FridgeId1");
            entity.Ignore("FridgeId2");
            entity.Ignore("FridgeId3");
            entity.Ignore("FridgeId4");
            entity.Ignore("FridgeId5");
            entity.Ignore("FridgeId6");
            entity.Ignore("FridgeId7");
            entity.Ignore("FridgeId8");
        });

        // 3. Configure Fridge relationships - UPDATED for string CustomerId
        builder.Entity<Fridge>()
            .HasIndex(f => f.SerialNumber)
            .IsUnique();

        builder.Entity<Fridge>()
            .Property(f => f.Price)
            .HasPrecision(16, 2);

        // UPDATED: CustomerId is now string? (nullable)
        builder.Entity<Fridge>()
            .HasOne(f => f.Customer)
            .WithMany(c => c.Fridges)
            .HasForeignKey(f => f.CustomerId)
            .IsRequired(false)  // Make it optional since CustomerId is nullable
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

        // 4. Configure Order relationships - CORRECTED (REMOVE Fridge relationship)
        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.ShippingFee)
                .HasPrecision(16, 2);

            // UPDATED: Customer relationship
            entity.HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(o => o.Invoices)
                  .WithOne(i => i.Order)
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            // REMOVED: Fridge relationship - This is causing the NULL FridgeId issue
            // Order should only relate to Fridges through OrderItems, not directly
            // entity.HasOne(o => o.Fridge)... 

            // Configure OrderItems relationship
            entity.HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. Configure OrderItem relationships
        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.UnitPrice)
                .HasPrecision(16, 2);

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Fridge)
                .WithMany(f => f.OrderItems)
                .HasForeignKey(oi => oi.FridgeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

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
            .HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.CreatedBy)
                .WithMany()
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // If you don't want any Suburb relationship, ignore all SuburbId properties
            entity.Ignore("SuburbId");
            entity.Ignore("SuburbId1");
            entity.Ignore("SuburbId2");
        });

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

        // 13. ScheduleMaintenance configuration - UPDATED for string CustomerId
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
        builder.Entity<ScheduleMaintenance>()
              .HasOne(s => s.Customer)
              .WithMany(c => c.ScheduleMaintenances)
              .HasForeignKey(s => s.CustomerId)
              .OnDelete(DeleteBehavior.Cascade);
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