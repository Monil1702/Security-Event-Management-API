using System.ComponentModel.DataAnnotations;

namespace SecurityEventManagement.Api.DTOs;

public sealed record CreateDeviceRequest(
    [property: Required, StringLength(120, MinimumLength = 2)] string Name,
    [property: Required, StringLength(80, MinimumLength = 2)] string DeviceType,
    [property: Required, StringLength(200, MinimumLength = 2)] string Location,
    [property: IpAddress] string? IpAddress,
    bool IsOnline);

public sealed record UpdateDeviceStatusRequest(bool IsOnline);

public sealed record SecurityDeviceResponse(
    Guid Id,
    string Name,
    string DeviceType,
    string Location,
    string? IpAddress,
    bool IsOnline,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset CreatedAt);
