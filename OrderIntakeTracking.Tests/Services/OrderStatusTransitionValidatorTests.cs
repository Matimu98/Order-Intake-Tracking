using OrderIntakeTracking.Application.Services;
using OrderIntakeTracking.Domain.Enums;

namespace OrderIntakeTracking.Tests.Services;

public class OrderStatusTransitionValidatorTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Fulfilled, true)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
    public void CanTransition_ReturnsExpectedResult(OrderStatus current, OrderStatus next, bool expected)
    {
        var result = OrderStatusTransitionValidator.CanTransition(current, next);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetTransitionErrorMessage_ForTerminalStatus_IsHelpful()
    {
        var message = OrderStatusTransitionValidator.GetTransitionErrorMessage(
            OrderStatus.Fulfilled,
            OrderStatus.Cancelled);

        Assert.Contains("Fulfilled", message);
        Assert.Contains("Cannot", message);
    }
}
