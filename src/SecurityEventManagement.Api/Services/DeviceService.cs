using Microsoft.EntityFrameworkCore;
using SecurityEventManagement.Api.Data;
using SecurityEventManagement.Api.Domain;
using SecurityEventManagement.Api.DTOs;
using SecurityEventManagement.Api.Exceptions;

namespace SecurityEventManagement.Api.Services;

public sealed class DeviceService(SecurityDbContext db) : IDeviceService
{
    public async Task<IReadOnlyCollection<SecurityDeviceResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var devices = await db.Devices
            .AsNoTracking()
            .OrderBy(x => x.Location)
            .ThenBy(x => x.Name)
            .ToArrayAsync(cancellationToken);
        return devices.Select(Map).ToArray();
    }

    public async Task<SecurityDeviceResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await db.Devices.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Device '{id}' was not found.");
        return Map(device);
    }

    public async Task<SecurityDeviceResponse> CreateAsync(
        CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var device = new SecurityDevice
        {
            Name = request.Name.Trim(),
            DeviceType = request.DeviceType.Trim(),
            Location = request.Location.Trim(),
            IpAddress = request.IpAddress,
            IsOnline = request.IsOnline,
            LastSeenAt = request.IsOnline ? DateTimeOffset.UtcNow : null
        };
        db.Devices.Add(device);
        await db.SaveChangesAsync(cancellationToken);
        return Map(device);
    }

    public async Task<SecurityDeviceResponse> UpdateStatusAsync(
        Guid id,
        UpdateDeviceStatusRequest request,
        CancellationToken cancellationToken)
    {
        var device = await db.Devices.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Device '{id}' was not found.");
        device.IsOnline = request.IsOnline;
        device.LastSeenAt = request.IsOnline ? DateTimeOffset.UtcNow : device.LastSeenAt;
        await db.SaveChangesAsync(cancellationToken);
        return Map(device);
    }

    private static SecurityDeviceResponse Map(SecurityDevice device) => new(
        device.Id,
        device.Name,
        device.DeviceType,
        device.Location,
        device.IpAddress,
        device.IsOnline,
        device.LastSeenAt,
        device.CreatedAt);
}
