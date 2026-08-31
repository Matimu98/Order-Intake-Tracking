using OrderIntakeTracking.Application.DTOs;

namespace OrderIntakeTracking.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderResponse>> GetOrdersAsync(CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
}
