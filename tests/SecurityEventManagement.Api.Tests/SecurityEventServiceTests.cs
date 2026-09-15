using Microsoft.EntityFrameworkCore;
using SecurityEventManagement.Api.Data;
using SecurityEventManagement.Api.Domain;
using SecurityEventManagement.Api.DTOs;
using SecurityEventManagement.Api.Exceptions;
using SecurityEventManagement.Api.Services;

namespace SecurityEventManagement.Api.Tests;

public sealed class SecurityEventServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidDevice_CreatesOpenEvent()
    {
        await using var db = CreateDatabase();
        var device = await AddDeviceAsync(db);
        var service = new SecurityEventService(db);

        var result = await service.CreateAsync(CreateRequest(device.Id), CancellationToken.None);

        Assert.Equal(EventStatus.Open, result.Status);
        Assert.Equal(device.Id, result.DeviceId);
        Assert.Equal(device.Name, result.DeviceName);
        Assert.Equal(1, await db.SecurityEvents.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WhenFingerprintExists_RejectsDuplicate()
    {
        await using var db = CreateDatabase();
        var device = await AddDeviceAsync(db);
        var service = new SecurityEventService(db);
        var request = CreateRequest(device.Id);
        await service.CreateAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<ResourceConflictException>(() =>
            service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ResolveAsync_AfterAcknowledgement_CompletesLifecycle()
    {
        await using var db = CreateDatabase();
        var device = await AddDeviceAsync(db);
        var service = new SecurityEventService(db);
        var created = await service.CreateAsync(CreateRequest(device.Id), CancellationToken.None);

        var acknowledged = await service.AcknowledgeAsync(
            created.Id,
            new AcknowledgeEventRequest("security.operator"),
            CancellationToken.None);
        var resolved = await service.ResolveAsync(
            created.Id,
            new ResolveEventRequest("Badge permissions were corrected."),
            CancellationToken.None);

        Assert.Equal(EventStatus.Acknowledged, acknowledged.Status);
        Assert.Equal(EventStatus.Resolved, resolved.Status);
        Assert.NotNull(resolved.ResolvedAt);
    }

    [Fact]
    public async Task ResolveAsync_WhenEventIsOpen_RejectsTransition()
    {
        await using var db = CreateDatabase();
        var device = await AddDeviceAsync(db);
        var service = new SecurityEventService(db);
        var created = await service.CreateAsync(CreateRequest(device.Id), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidStateTransitionException>(() =>
            service.ResolveAsync(
                created.Id,
                new ResolveEventRequest("Attempted premature resolution."),
                CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_WithSeverityFilter_ReturnsMatchingEvents()
    {
        await using var db = CreateDatabase();
        var device = await AddDeviceAsync(db);
        var service = new SecurityEventService(db);
        await service.CreateAsync(CreateRequest(device.Id), CancellationToken.None);
        await service.CreateAsync(
            CreateRequest(device.Id) with
            {
                EventType = SecurityEventType.CameraOffline,
                Severity = EventSeverity.Medium,
                Description = "Camera heartbeat was not received."
            },
            CancellationToken.None);

        var result = await service.SearchAsync(
            null,
            EventSeverity.High,
            null,
            null,
            null,
            null,
            1,
            20,
            CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(EventSeverity.High, result.Items.Single().Severity);
    }

    private static SecurityDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<SecurityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SecurityDbContext(options);
    }

    private static async Task<SecurityDevice> AddDeviceAsync(SecurityDbContext db)
    {
        var device = new SecurityDevice
        {
            Name = "North Door Reader",
            DeviceType = "Access Controller",
            Location = "Building A"
        };
        db.Devices.Add(device);
        await db.SaveChangesAsync();
        return device;
    }

    private static CreateSecurityEventRequest CreateRequest(Guid deviceId) => new(
        deviceId,
        SecurityEventType.AccessDenied,
        EventSeverity.High,
        "Repeated access denial detected.",
        "access-controller",
        DateTimeOffset.UtcNow.AddMinutes(-1));
}
