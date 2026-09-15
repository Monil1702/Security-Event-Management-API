using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SecurityEventManagement.Api.Data;
using SecurityEventManagement.Api.Domain;
using SecurityEventManagement.Api.DTOs;
using SecurityEventManagement.Api.Exceptions;

namespace SecurityEventManagement.Api.Services;

public sealed class SecurityEventService(SecurityDbContext db) : ISecurityEventService
{
    public async Task<SecurityEventResponse> CreateAsync(
        CreateSecurityEventRequest request,
        CancellationToken cancellationToken)
    {
        var device = await db.Devices
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.DeviceId, cancellationToken)
            ?? throw new ResourceNotFoundException($"Device '{request.DeviceId}' was not found.");

        if (request.OccurredAt > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            throw new ArgumentException("OccurredAt cannot be more than five minutes in the future.");
        }

        var fingerprint = CreateFingerprint(request);
        if (await db.SecurityEvents.AnyAsync(x => x.Fingerprint == fingerprint, cancellationToken))
        {
            throw new ResourceConflictException("A matching security event has already been received.");
        }

        var securityEvent = new SecurityEvent
        {
            DeviceId = request.DeviceId,
            EventType = request.EventType,
            Severity = request.Severity,
            Description = request.Description.Trim(),
            Source = request.Source.Trim(),
            OccurredAt = request.OccurredAt.ToUniversalTime(),
            Fingerprint = fingerprint
        };

        db.SecurityEvents.Add(securityEvent);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ResourceConflictException("A matching security event has already been received.");
        }
        return Map(securityEvent, device.Name);
    }

    public async Task<SecurityEventResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var securityEvent = await EventQuery()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ResourceNotFoundException($"Security event '{id}' was not found.");

        return Map(securityEvent);
    }

    public async Task<PagedResult<SecurityEventResponse>> SearchAsync(
        EventStatus? status,
        EventSeverity? severity,
        SecurityEventType? eventType,
        Guid? deviceId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            throw new ArgumentException("Page must be positive and pageSize must be between 1 and 100.");
        }

        if (from.HasValue && to.HasValue && from > to)
        {
            throw new ArgumentException("The from timestamp must be earlier than the to timestamp.");
        }

        var query = EventQuery();
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (severity.HasValue) query = query.Where(x => x.Severity == severity.Value);
        if (eventType.HasValue) query = query.Where(x => x.EventType == eventType.Value);
        if (deviceId.HasValue) query = query.Where(x => x.DeviceId == deviceId.Value);
        if (from.HasValue) query = query.Where(x => x.OccurredAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.OccurredAt <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var events = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SecurityEventResponse>(events.Select(x => Map(x)).ToArray(), page, pageSize, totalCount);
    }

    public async Task<SecurityEventResponse> AcknowledgeAsync(
        Guid id,
        AcknowledgeEventRequest request,
        CancellationToken cancellationToken)
    {
        var securityEvent = await FindTrackedAsync(id, cancellationToken);
        if (securityEvent.Status != EventStatus.Open)
        {
            throw new InvalidStateTransitionException("Only open events can be acknowledged.");
        }

        securityEvent.Status = EventStatus.Acknowledged;
        securityEvent.AcknowledgedAt = DateTimeOffset.UtcNow;
        securityEvent.AcknowledgedBy = request.AcknowledgedBy.Trim();
        securityEvent.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Map(securityEvent);
    }

    public async Task<SecurityEventResponse> ResolveAsync(
        Guid id,
        ResolveEventRequest request,
        CancellationToken cancellationToken)
    {
        var securityEvent = await FindTrackedAsync(id, cancellationToken);
        if (securityEvent.Status != EventStatus.Acknowledged)
        {
            throw new InvalidStateTransitionException("An event must be acknowledged before it can be resolved.");
        }

        securityEvent.Status = EventStatus.Resolved;
        securityEvent.ResolvedAt = DateTimeOffset.UtcNow;
        securityEvent.ResolutionNotes = request.ResolutionNotes.Trim();
        securityEvent.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Map(securityEvent);
    }

    public async Task<EventSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var counts = await db.SecurityEvents
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        var critical = await db.SecurityEvents.CountAsync(x => x.Severity == EventSeverity.Critical, cancellationToken);
        var byType = await db.SecurityEvents
            .AsNoTracking()
            .GroupBy(x => x.EventType)
            .Select(group => new { Type = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Type.ToString(), x => x.Count, cancellationToken);

        return new EventSummaryResponse(
            counts.Values.Sum(),
            counts.GetValueOrDefault(EventStatus.Open),
            counts.GetValueOrDefault(EventStatus.Acknowledged),
            counts.GetValueOrDefault(EventStatus.Resolved),
            critical,
            byType);
    }

    private IQueryable<SecurityEvent> EventQuery() =>
        db.SecurityEvents.AsNoTracking().Include(x => x.Device);

    private async Task<SecurityEvent> FindTrackedAsync(Guid id, CancellationToken cancellationToken) =>
        await db.SecurityEvents.Include(x => x.Device).SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new ResourceNotFoundException($"Security event '{id}' was not found.");

    private static string CreateFingerprint(CreateSecurityEventRequest request)
    {
        var normalized = string.Join('|',
            request.DeviceId,
            request.EventType,
            request.Source.Trim().ToUpperInvariant(),
            request.Description.Trim().ToUpperInvariant(),
            request.OccurredAt.ToUniversalTime().ToUnixTimeSeconds());
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    private static SecurityEventResponse Map(SecurityEvent securityEvent, string? deviceName = null) => new(
        securityEvent.Id,
        securityEvent.DeviceId,
        deviceName ?? securityEvent.Device?.Name ?? "Unknown device",
        securityEvent.EventType,
        securityEvent.Severity,
        securityEvent.Status,
        securityEvent.Description,
        securityEvent.Source,
        securityEvent.OccurredAt,
        securityEvent.ReceivedAt,
        securityEvent.AcknowledgedAt,
        securityEvent.AcknowledgedBy,
        securityEvent.ResolvedAt,
        securityEvent.ResolutionNotes);
}
