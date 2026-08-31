using OrderIntakeTracking.Domain.Enums;

namespace OrderIntakeTracking.Application.DTOs;

public record OrderResponse(
    Guid Id,
    string ExternalReference,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    OrderStatus Status,
    string Currency,
    string? Notes,
    CustomerDto Customer,
    IReadOnlyList<LineItemResponse> LineItems,
    decimal Subtotal,
    decimal Total,
    bool WasDuplicate = false);
