using Microsoft.EntityFrameworkCore;
using SecurityEventManagement.Api.Data;
using SecurityEventManagement.Api.DTOs;
using SecurityEventManagement.Api.Services;

namespace SecurityEventManagement.Api.Tests;

public sealed class DeviceServiceTests
{
    [Fact]
    public async Task UpdateStatusAsync_WhenDeviceComesOnline_RecordsLastSeen()
    {
        var options = new DbContextOptionsBuilder<SecurityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new SecurityDbContext(options);
        var service = new DeviceService(db);
        var created = await service.CreateAsync(
            new CreateDeviceRequest("Lobby Camera", "IP Camera", "Main Lobby", "10.0.0.20", false),
            CancellationToken.None);

        var updated = await service.UpdateStatusAsync(
            created.Id,
            new UpdateDeviceStatusRequest(true),
            CancellationToken.None);

        Assert.True(updated.IsOnline);
        Assert.NotNull(updated.LastSeenAt);
    }
}
