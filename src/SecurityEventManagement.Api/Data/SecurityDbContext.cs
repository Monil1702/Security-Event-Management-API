using Microsoft.EntityFrameworkCore;
using SecurityEventManagement.Api.Domain;

namespace SecurityEventManagement.Api.Data;

public sealed class SecurityDbContext(DbContextOptions<SecurityDbContext> options)
    : DbContext(options)
{
    public DbSet<SecurityDevice> Devices => Set<SecurityDevice>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var device = modelBuilder.Entity<SecurityDevice>();
        device.ToTable("security_devices");
        device.HasKey(x => x.Id);
        device.Property(x => x.Name).HasMaxLength(120).IsRequired();
        device.Property(x => x.DeviceType).HasMaxLength(80).IsRequired();
        device.Property(x => x.Location).HasMaxLength(200).IsRequired();
        device.Property(x => x.IpAddress).HasMaxLength(45);
        device.HasIndex(x => x.Name);

        var securityEvent = modelBuilder.Entity<SecurityEvent>();
        securityEvent.ToTable("security_events");
        securityEvent.HasKey(x => x.Id);
        securityEvent.Property(x => x.EventType).HasConversion<string>().HasMaxLength(60);
        securityEvent.Property(x => x.Severity).HasConversion<string>().HasMaxLength(20);
        securityEvent.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        securityEvent.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        securityEvent.Property(x => x.Source).HasMaxLength(120).IsRequired();
        securityEvent.Property(x => x.Fingerprint).HasMaxLength(64).IsRequired();
        securityEvent.Property(x => x.AcknowledgedBy).HasMaxLength(120);
        securityEvent.Property(x => x.ResolutionNotes).HasMaxLength(1000);
        securityEvent.HasIndex(x => x.Fingerprint).IsUnique();
        securityEvent.HasIndex(x => new { x.Status, x.Severity, x.OccurredAt });
        securityEvent.HasOne(x => x.Device)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
