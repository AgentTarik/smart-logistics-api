using Microsoft.EntityFrameworkCore;
using SmartLogistics.Domain.Entities;

namespace SmartLogistics.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<ZoneConnection> ZoneConnections => Set<ZoneConnection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Driver
        modelBuilder.Entity<Driver>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Name).HasMaxLength(200).IsRequired();
            e.Property(d => d.Email).HasMaxLength(300).IsRequired();
            e.HasIndex(d => d.Email).IsUnique();
            e.Property(d => d.CurrentLocation).HasColumnType("geography (point)");
        });

        // Merchant
        modelBuilder.Entity<Merchant>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Name).HasMaxLength(200).IsRequired();
            e.Property(m => m.ApiKey).HasMaxLength(64).IsRequired();
            e.HasIndex(m => m.ApiKey).IsUnique();
            e.Property(m => m.Location).HasColumnType("geography (point)");
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.PickupLocation).HasColumnType("geography (point)");
            e.Property(o => o.DeliveryLocation).HasColumnType("geography (point)");
            e.HasOne(o => o.Merchant)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(o => o.Delivery)
                .WithOne(d => d.Order)
                .HasForeignKey<Delivery>(d => d.OrderId);
        });

        // Delivery
        modelBuilder.Entity<Delivery>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.Driver)
                .WithMany(dr => dr.Deliveries)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Zone
        modelBuilder.Entity<Zone>(e =>
        {
            e.HasKey(z => z.Id);
            e.Property(z => z.Name).HasMaxLength(100).IsRequired();
            e.Property(z => z.Boundary).HasColumnType("geography (polygon)");
        });

        // ZoneConnection (graph edges)
        modelBuilder.Entity<ZoneConnection>(e =>
        {
            e.HasKey(zc => zc.Id);
            e.HasIndex(zc => new { zc.FromZoneId, zc.ToZoneId }).IsUnique();
            e.HasOne(zc => zc.FromZone)
                .WithMany(z => z.OutgoingConnections)
                .HasForeignKey(zc => zc.FromZoneId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(zc => zc.ToZone)
                .WithMany(z => z.IncomingConnections)
                .HasForeignKey(zc => zc.ToZoneId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}