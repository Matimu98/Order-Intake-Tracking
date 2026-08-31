using OrderIntakeTracking.Application.DTOs;
using OrderIntakeTracking.Application.Interfaces;
using OrderIntakeTracking.Application.Services;
using OrderIntakeTracking.Domain.Enums;
using OrderIntakeTracking.Infrastructure.Repositories;

namespace OrderIntakeTracking.Tests.Services;

public class OrderServiceTests
{
    private readonly IOrderRepository _repository = new InMemoryOrderRepository();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_repository);
    }

    [Fact]
    public async Task CreateOrderAsync_CreatesOrderWithServerCalculatedTotals()
    {
        var request = CreateValidRequest("REF-001");

        var result = await _sut.CreateOrderAsync(request);

        Assert.False(result.WasDuplicate);
        Assert.Equal(150m, result.Subtotal);
        Assert.Equal(150m, result.Total);
        Assert.Equal(50m, result.LineItems[0].LineTotal);
        Assert.Equal(100m, result.LineItems[1].LineTotal);
        Assert.Equal(OrderStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateOrderAsync_WithDuplicateExternalReference_ReturnsExistingOrder()
    {
        var request = CreateValidRequest("REF-DUP");

        var first = await _sut.CreateOrderAsync(request);
        var second = await _sut.CreateOrderAsync(request);

        Assert.False(first.WasDuplicate);
        Assert.True(second.WasDuplicate);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal(first.ExternalReference, second.ExternalReference);
    }

    [Fact]
    public async Task CreateOrderAsync_WithDuplicateReference_IsCaseInsensitive()
    {
        var first = await _sut.CreateOrderAsync(CreateValidRequest("ref-case"));
        var second = await _sut.CreateOrderAsync(CreateValidRequest("REF-CASE"));

        Assert.True(second.WasDuplicate);
        Assert.Equal(first.Id, second.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateOrderAsync_WithInvalidQuantity_ThrowsValidationException(int quantity)
    {
        var request = CreateValidRequest("REF-INVALID-QTY") with
        {
            LineItems = [new LineItemDto("SKU-1", "Widget", quantity, 10m)]
        };

        await Assert.ThrowsAsync<Application.Exceptions.ValidationException>(
            () => _sut.CreateOrderAsync(request));
    }

    [Fact]
    public async Task CreateOrderAsync_WithNegativePrice_ThrowsValidationException()
    {
        var request = CreateValidRequest("REF-INVALID-PRICE") with
        {
            LineItems = [new LineItemDto("SKU-1", "Widget", 1, -5m)]
        };

        await Assert.ThrowsAsync<Application.Exceptions.ValidationException>(
            () => _sut.CreateOrderAsync(request));
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsOrdersNewestFirst()
    {
        await _sut.CreateOrderAsync(CreateValidRequest("REF-OLD"));
        await Task.Delay(10);
        await _sut.CreateOrderAsync(CreateValidRequest("REF-NEW"));

        var orders = await _sut.GetOrdersAsync();

        Assert.Equal(2, orders.Count);
        Assert.Equal("REF-NEW", orders[0].ExternalReference);
        Assert.Equal("REF-OLD", orders[1].ExternalReference);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Fulfilled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Fulfilled, false)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
    public async Task UpdateOrderStatusAsync_EnforcesValidTransitions(
        OrderStatus current,
        OrderStatus next,
        bool shouldSucceed)
    {
        var created = await _sut.CreateOrderAsync(CreateValidRequest($"REF-{current}-{next}"));
        await MoveOrderToStatusAsync(created.Id, current);

        if (shouldSucceed)
        {
            var updated = await _sut.UpdateOrderStatusAsync(
                created.Id,
                new UpdateOrderStatusRequest(next));

            Assert.Equal(next, updated.Status);
            Assert.NotNull(updated.UpdatedAt);
        }
        else
        {
            await Assert.ThrowsAsync<Application.Exceptions.InvalidStatusTransitionException>(
                () => _sut.UpdateOrderStatusAsync(created.Id, new UpdateOrderStatusRequest(next)));
        }
    }

    private async Task MoveOrderToStatusAsync(Guid orderId, OrderStatus targetStatus)
    {
        var transitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Pending] = [],
            [OrderStatus.Confirmed] = [OrderStatus.Confirmed],
            [OrderStatus.Fulfilled] = [OrderStatus.Confirmed, OrderStatus.Fulfilled],
            [OrderStatus.Cancelled] = [OrderStatus.Cancelled]
        };

        foreach (var status in transitions[targetStatus])
        {
            await _sut.UpdateOrderStatusAsync(orderId, new UpdateOrderStatusRequest(status));
        }
    }

    private static CreateOrderRequest CreateValidRequest(string externalReference) =>
        new(
            externalReference,
            "USD",
            "Test order",
            new CustomerDto("Jane Doe", "jane@example.com"),
            [
                new LineItemDto("SKU-1", "Widget A", 2, 25m),
                new LineItemDto("SKU-2", "Widget B", 1, 100m)
            ]);
}
