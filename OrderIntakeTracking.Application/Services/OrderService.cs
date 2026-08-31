using OrderIntakeTracking.Application.DTOs;
using OrderIntakeTracking.Application.Exceptions;
using OrderIntakeTracking.Application.Interfaces;
using OrderIntakeTracking.Domain.Entities;
using OrderIntakeTracking.Domain.Enums;

namespace OrderIntakeTracking.Application.Services;

public class OrderService(IOrderRepository orderRepository) : IOrderService
{
    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        var normalizedReference = request.ExternalReference.Trim();
        var existingOrder = await orderRepository.GetByExternalReferenceAsync(normalizedReference, cancellationToken);
        if (existingOrder is not null)
        {
            return OrderMapper.ToResponse(existingOrder, wasDuplicate: true);
        }

        var lineItems = request.LineItems.Select(item => new LineItem
        {
            Id = Guid.NewGuid(),
            Sku = item.Sku.Trim(),
            Name = item.Name.Trim(),
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = OrderTotalsCalculator.CalculateLineTotal(item.Quantity, item.UnitPrice)
        }).ToList();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ExternalReference = normalizedReference,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = request.Customer.Name.Trim(),
                Email = request.Customer.Email.Trim()
            },
            LineItems = lineItems
        };

        OrderTotalsCalculator.ApplyTotals(order);
        await orderRepository.AddAsync(order, cancellationToken);

        return OrderMapper.ToResponse(order);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : OrderMapper.ToResponse(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await orderRepository.GetAllAsync(cancellationToken);
        return orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => OrderMapper.ToResponse(o))
            .ToList();
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Order with id '{id}' was not found.");

        if (!OrderStatusTransitionValidator.CanTransition(order.Status, request.NewStatus))
        {
            throw new InvalidStatusTransitionException(
                OrderStatusTransitionValidator.GetTransitionErrorMessage(order.Status, request.NewStatus));
        }

        order.Status = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await orderRepository.UpdateAsync(order, cancellationToken);

        return OrderMapper.ToResponse(order);
    }

    private static void ValidateCreateRequest(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExternalReference))
        {
            throw new ValidationException("External reference is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            throw new ValidationException("Currency is required.");
        }

        if (request.Customer is null || string.IsNullOrWhiteSpace(request.Customer.Name))
        {
            throw new ValidationException("Customer name is required.");
        }

        if (request.Customer is null || string.IsNullOrWhiteSpace(request.Customer.Email))
        {
            throw new ValidationException("Customer email is required.");
        }

        if (request.LineItems is null || request.LineItems.Count == 0)
        {
            throw new ValidationException("At least one line item is required.");
        }

        foreach (var item in request.LineItems)
        {
            if (string.IsNullOrWhiteSpace(item.Sku))
            {
                throw new ValidationException("Line item SKU is required.");
            }

            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new ValidationException("Line item name is required.");
            }

            if (item.Quantity <= 0)
            {
                throw new ValidationException($"Quantity for SKU '{item.Sku}' must be a positive whole number.");
            }

            if (item.UnitPrice < 0)
            {
                throw new ValidationException($"Unit price for SKU '{item.Sku}' cannot be negative.");
            }
        }
    }
}
