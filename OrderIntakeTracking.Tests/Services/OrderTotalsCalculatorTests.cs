using OrderIntakeTracking.Application.DTOs;
using OrderIntakeTracking.Application.Services;

namespace OrderIntakeTracking.Tests.Services;

public class OrderTotalsCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsCorrectSubtotalAndTotal()
    {
        var lineItems = new List<LineItemDto>
        {
            new("SKU-1", "Item 1", 3, 10m),
            new("SKU-2", "Item 2", 2, 25.5m)
        };

        var (subtotal, total) = OrderTotalsCalculator.Calculate(lineItems);

        Assert.Equal(81m, subtotal);
        Assert.Equal(81m, total);
    }

    [Fact]
    public void CalculateLineTotal_MultipliesQuantityAndUnitPrice()
    {
        var lineTotal = OrderTotalsCalculator.CalculateLineTotal(4, 12.5m);

        Assert.Equal(50m, lineTotal);
    }
}
