using OrderIntakeTracking.Application.DTOs;
using OrderIntakeTracking.Domain.Entities;

namespace OrderIntakeTracking.Application.Services;

public static class OrderMapper
{
    public static OrderResponse ToResponse(Order order, bool wasDuplicate = false) =>
        new(
            order.Id,
            order.ExternalReference,
            order.CreatedAt,
            order.UpdatedAt,
            order.Status,
            order.Currency,
            order.Notes,
            new CustomerDto(order.Customer.Name, order.Customer.Email),
            order.LineItems.Select(item => new LineItemResponse(
                item.Id,
                item.Sku,
                item.Name,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal)).ToList(),
            order.Subtotal,
            order.Total,
            wasDuplicate);
}
