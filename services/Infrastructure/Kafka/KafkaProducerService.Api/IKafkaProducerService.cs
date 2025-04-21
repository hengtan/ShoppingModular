namespace KafkaProducerService.Api;

public interface IKafkaProducerService
{
    Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default);
}