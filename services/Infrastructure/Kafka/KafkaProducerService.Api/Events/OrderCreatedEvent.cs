namespace KafkaProducerService.Api.Events;

public class OrderCreatedEvent
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
}