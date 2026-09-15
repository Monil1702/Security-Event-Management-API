using SecurityEventManagement.Api.Domain;
using SecurityEventManagement.Api.DTOs;

namespace SecurityEventManagement.Api.Services;

public interface ISecurityEventService
{
    Task<SecurityEventResponse> CreateAsync(CreateSecurityEventRequest request, CancellationToken cancellationToken);
    Task<SecurityEventResponse> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<SecurityEventResponse>> SearchAsync(
        EventStatus? status,
        EventSeverity? severity,
        SecurityEventType? eventType,
        Guid? deviceId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    Task<SecurityEventResponse> AcknowledgeAsync(Guid id, AcknowledgeEventRequest request, CancellationToken cancellationToken);
    Task<SecurityEventResponse> ResolveAsync(Guid id, ResolveEventRequest request, CancellationToken cancellationToken);
    Task<EventSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
}
