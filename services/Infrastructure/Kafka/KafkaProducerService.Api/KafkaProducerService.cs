using System.Text.Json;
using Confluent.Kafka;

namespace KafkaProducerService.Api;

/// <summary>
/// Serviço responsável por publicar eventos no Kafka.
/// </summary>
public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerService(IConfiguration configuration)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<Null, string> { Value = json };

        await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
    }
}