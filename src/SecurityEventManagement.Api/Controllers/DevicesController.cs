using Microsoft.AspNetCore.Mvc;
using SecurityEventManagement.Api.DTOs;
using SecurityEventManagement.Api.Services;

namespace SecurityEventManagement.Api.Controllers;

[ApiController]
[Route("api/v1/devices")]
public sealed class DevicesController(IDeviceService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<SecurityDeviceResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<SecurityDeviceResponse>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<SecurityDeviceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SecurityDeviceResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<SecurityDeviceResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<SecurityDeviceResponse>> Create(
        CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<SecurityDeviceResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SecurityDeviceResponse>> UpdateStatus(
        Guid id,
        UpdateDeviceStatusRequest request,
        CancellationToken cancellationToken) =>
        Ok(await service.UpdateStatusAsync(id, request, cancellationToken));
}
