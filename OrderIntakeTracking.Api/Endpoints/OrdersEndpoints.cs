using OrderIntakeTracking.Application.DTOs;
using OrderIntakeTracking.Application.Exceptions;
using OrderIntakeTracking.Application.Interfaces;

namespace OrderIntakeTracking.Api.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .WithOpenApi();

        group.MapGet("/", GetOrders)
            .WithName("GetOrders")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetOrderById)
            .WithName("GetOrderById")
            .WithOpenApi();

        group.MapPatch("/{id:guid}/status", UpdateOrderStatus)
            .WithName("UpdateOrderStatus")
            .WithOpenApi();
    }

    private static async Task<IResult> CreateOrder(
        CreateOrderRequest request,
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.CreateOrderAsync(request, cancellationToken);
            return order.WasDuplicate
                ? Results.Ok(order)
                : Results.Created($"/api/orders/{order.Id}", order);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetOrders(
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        var orders = await orderService.GetOrdersAsync(cancellationToken);
        return Results.Ok(orders);
    }

    private static async Task<IResult> GetOrderById(
        Guid id,
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        return order is null ? Results.NotFound(new { error = $"Order with id '{id}' was not found." }) : Results.Ok(order);
    }

    private static async Task<IResult> UpdateOrderStatus(
        Guid id,
        UpdateOrderStatusRequest request,
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.UpdateOrderStatusAsync(id, request, cancellationToken);
            return Results.Ok(order);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidStatusTransitionException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
