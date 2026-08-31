namespace OrderIntakeTracking.Application.DTOs;

public record LineItemResponse(
    Guid Id,
    string Sku,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
