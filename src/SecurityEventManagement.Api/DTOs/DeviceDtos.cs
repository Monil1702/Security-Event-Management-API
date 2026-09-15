using System.ComponentModel.DataAnnotations;
using System.Net;

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

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class IpAddressAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is null || value is string address && IPAddress.TryParse(address, out _);
}
