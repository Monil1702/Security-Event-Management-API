using Microsoft.AspNetCore.Mvc;
using SecurityEventManagement.Api.Domain;
using SecurityEventManagement.Api.DTOs;
using SecurityEventManagement.Api.Services;

namespace SecurityEventManagement.Api.Controllers;

[ApiController]
[Route("api/v1/security-events")]
public sealed class SecurityEventsController(ISecurityEventService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<SecurityEventResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SecurityEventResponse>> Create(
        CreateSecurityEventRequest request,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<SecurityEventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SecurityEventResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(id, cancellationToken));

    [HttpGet]
    [ProducesResponseType<PagedResult<SecurityEventResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SecurityEventResponse>>> Search(
        EventStatus? status,
        EventSeverity? severity,
        SecurityEventType? eventType,
        Guid? deviceId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await service.SearchAsync(status, severity, eventType, deviceId, from, to, page, pageSize, cancellationToken));

    [HttpPost("{id:guid}/acknowledge")]
    [ProducesResponseType<SecurityEventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SecurityEventResponse>> Acknowledge(
        Guid id,
        AcknowledgeEventRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.AcknowledgeAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/resolve")]
    [ProducesResponseType<SecurityEventResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SecurityEventResponse>> Resolve(
        Guid id,
        ResolveEventRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.ResolveAsync(id, request, cancellationToken));

    [HttpGet("summary")]
    [ProducesResponseType<EventSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EventSummaryResponse>> Summary(CancellationToken cancellationToken) =>
        Ok(await service.GetSummaryAsync(cancellationToken));
}
