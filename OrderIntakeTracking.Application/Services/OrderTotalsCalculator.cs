using OrderIntakeTracking.Application.DTOs;
using OrderIntakeTracking.Domain.Entities;

namespace OrderIntakeTracking.Application.Services;

public static class OrderTotalsCalculator
{
    public static (decimal Subtotal, decimal Total) Calculate(IReadOnlyList<LineItemDto> lineItems)
    {
        var subtotal = lineItems.Sum(item => item.Quantity * item.UnitPrice);
        return (subtotal, subtotal);
    }

    public static void ApplyTotals(Order order)
    {
        order.Subtotal = order.LineItems.Sum(item => item.LineTotal);
        order.Total = order.Subtotal;
    }

    public static decimal CalculateLineTotal(int quantity, decimal unitPrice) =>
        quantity * unitPrice;
}
