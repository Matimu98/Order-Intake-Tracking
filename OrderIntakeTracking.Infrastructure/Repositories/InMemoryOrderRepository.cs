using System.Collections.Concurrent;
using OrderIntakeTracking.Application.Interfaces;
using OrderIntakeTracking.Domain.Entities;

namespace OrderIntakeTracking.Infrastructure.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _ordersById = new();
    private readonly ConcurrentDictionary<string, Guid> _idsByExternalReference = new(StringComparer.OrdinalIgnoreCase);

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (!_ordersById.TryAdd(order.Id, order))
        {
            throw new InvalidOperationException($"Order with id '{order.Id}' already exists.");
        }

        if (!_idsByExternalReference.TryAdd(order.ExternalReference, order.Id))
        {
            _ordersById.TryRemove(order.Id, out _);
            throw new InvalidOperationException($"Order with external reference '{order.ExternalReference}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _ordersById.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task<Order?> GetByExternalReferenceAsync(string externalReference, CancellationToken cancellationToken = default)
    {
        if (!_idsByExternalReference.TryGetValue(externalReference, out var id))
        {
            return Task.FromResult<Order?>(null);
        }

        _ordersById.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Order> orders = _ordersById.Values.ToList();
        return Task.FromResult(orders);
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (!_ordersById.ContainsKey(order.Id))
        {
            throw new KeyNotFoundException($"Order with id '{order.Id}' was not found.");
        }

        _ordersById[order.Id] = order;
        return Task.CompletedTask;
    }
}
