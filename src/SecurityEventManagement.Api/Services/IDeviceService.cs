using SecurityEventManagement.Api.DTOs;

namespace SecurityEventManagement.Api.Services;

public interface IDeviceService
{
    Task<IReadOnlyCollection<SecurityDeviceResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<SecurityDeviceResponse> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<SecurityDeviceResponse> CreateAsync(CreateDeviceRequest request, CancellationToken cancellationToken);
    Task<SecurityDeviceResponse> UpdateStatusAsync(Guid id, UpdateDeviceStatusRequest request, CancellationToken cancellationToken);
}
