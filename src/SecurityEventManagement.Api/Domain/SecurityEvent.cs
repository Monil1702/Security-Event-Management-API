namespace SecurityEventManagement.Api.Domain;

public sealed class SecurityEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeviceId { get; set; }
    public SecurityDevice? Device { get; set; }
    public SecurityEventType EventType { get; set; }
    public EventSeverity Severity { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Open;
    public required string Description { get; set; }
    public required string Source { get; set; }
    public required string Fingerprint { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}

public enum SecurityEventType
{
    AccessDenied,
    ForcedDoor,
    MotionDetected,
    CameraOffline,
    LicensePlateMatch,
    DeviceTampering,
    AuthenticationFailure,
    Other
}

public enum EventSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum EventStatus
{
    Open,
    Acknowledged,
    Resolved
}
