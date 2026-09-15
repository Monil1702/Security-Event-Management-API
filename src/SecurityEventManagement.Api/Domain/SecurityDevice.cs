namespace SecurityEventManagement.Api.Domain;

public sealed class SecurityDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string DeviceType { get; set; }
    public required string Location { get; set; }
    public string? IpAddress { get; set; }
    public bool IsOnline { get; set; }
    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<SecurityEvent> Events { get; set; } = new List<SecurityEvent>();
}
