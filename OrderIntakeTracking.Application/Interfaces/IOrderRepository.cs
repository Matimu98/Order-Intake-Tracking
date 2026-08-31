namespace OrderIntakeTracking.Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Domain.Entities.Order order, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Order?> GetByExternalReferenceAsync(string externalReference, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.Order order, CancellationToken cancellationToken = default);
}
