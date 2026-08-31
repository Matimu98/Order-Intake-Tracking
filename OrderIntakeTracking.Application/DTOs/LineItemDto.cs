namespace OrderIntakeTracking.Application.DTOs;

public record LineItemDto(string Sku, string Name, int Quantity, decimal UnitPrice);
