using Microsoft.EntityFrameworkCore;
using SecurityEventManagement.Api.Domain;

namespace SecurityEventManagement.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(SecurityDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);
        if (await db.Devices.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Devices.AddRange(
            new SecurityDevice
            {
                Name = "Main Entrance Reader",
                DeviceType = "Access Controller",
                Location = "Montreal HQ - Lobby",
                IpAddress = "10.0.10.21",
                IsOnline = true,
                LastSeenAt = DateTimeOffset.UtcNow
            },
            new SecurityDevice
            {
                Name = "Parking Camera 01",
                DeviceType = "IP Camera",
                Location = "Montreal HQ - Parking",
                IpAddress = "10.0.20.31",
                IsOnline = true,
                LastSeenAt = DateTimeOffset.UtcNow
            });

        await db.SaveChangesAsync(cancellationToken);
    }
}
