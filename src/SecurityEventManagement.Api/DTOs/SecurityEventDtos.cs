using System.ComponentModel.DataAnnotations;
using SecurityEventManagement.Api.Domain;

namespace SecurityEventManagement.Api.DTOs;

public sealed record CreateSecurityEventRequest(
    [property: Required] Guid DeviceId,
    [property: Required] SecurityEventType EventType,
    [property: Required] EventSeverity Severity,
    [property: Required, StringLength(1000, MinimumLength = 5)] string Description,
    [property: Required, StringLength(120)] string Source,
    [property: Required] DateTimeOffset OccurredAt);

public sealed record AcknowledgeEventRequest(
    [property: Required, StringLength(120, MinimumLength = 2)] string AcknowledgedBy);

public sealed record ResolveEventRequest(
    [property: Required, StringLength(1000, MinimumLength = 5)] string ResolutionNotes);

public sealed record SecurityEventResponse(
    Guid Id,
    Guid DeviceId,
    string DeviceName,
    SecurityEventType EventType,
    EventSeverity Severity,
    EventStatus Status,
    string Description,
    string Source,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? AcknowledgedAt,
    string? AcknowledgedBy,
    DateTimeOffset? ResolvedAt,
    string? ResolutionNotes);

public sealed record EventSummaryResponse(
    int Total,
    int Open,
    int Acknowledged,
    int Resolved,
    int Critical,
    IReadOnlyDictionary<string, int> ByType);

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
