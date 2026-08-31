using OrderIntakeTracking.Domain.Enums;

namespace OrderIntakeTracking.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public required string ExternalReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public required string Currency { get; set; }
    public string? Notes { get; set; }
    public required Customer Customer { get; set; }
    public List<LineItem> LineItems { get; set; } = [];
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
}
