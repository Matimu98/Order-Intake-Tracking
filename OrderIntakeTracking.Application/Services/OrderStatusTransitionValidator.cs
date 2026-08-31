using OrderIntakeTracking.Domain.Enums;

namespace OrderIntakeTracking.Application.Services;

public static class OrderStatusTransitionValidator
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Fulfilled, OrderStatus.Cancelled],
        [OrderStatus.Fulfilled] = [],
        [OrderStatus.Cancelled] = []
    };

    public static bool CanTransition(OrderStatus current, OrderStatus next) =>
        AllowedTransitions.TryGetValue(current, out var allowed) && allowed.Contains(next);

    public static string GetTransitionErrorMessage(OrderStatus current, OrderStatus next)
    {
        if (current == next)
        {
            return $"Order is already in '{current}' status.";
        }

        if (current is OrderStatus.Fulfilled or OrderStatus.Cancelled)
        {
            return $"Cannot change status of a '{current}' order.";
        }

        return $"Cannot transition from '{current}' to '{next}'. " +
               $"Allowed transitions from '{current}': {string.Join(", ", AllowedTransitions[current])}.";
    }
}
