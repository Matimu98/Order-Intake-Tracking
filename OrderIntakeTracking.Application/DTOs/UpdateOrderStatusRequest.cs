using OrderIntakeTracking.Domain.Enums;

namespace OrderIntakeTracking.Application.DTOs;

public record UpdateOrderStatusRequest(OrderStatus NewStatus);
