using OrderIntakeTracking.Domain.Enums;

namespace OrderIntakeTracking.Application.DTOs;

public record CreateOrderRequest(
    string ExternalReference,
    string Currency,
    string? Notes,
    CustomerDto Customer,
    IReadOnlyList<LineItemDto> LineItems);
