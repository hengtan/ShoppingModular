using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace KafkaProducerService;

/// <summary>
/// Serviço responsável por publicar eventos genéricos no Kafka.
/// </summary>
public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerService(IConfiguration configuration)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"];

        Console.WriteLine($"📡 Kafka BootstrapServers: {bootstrapServers}");

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            ClientId = "orders-api",
            SocketKeepaliveEnable = true,
            MetadataMaxAgeMs = 5000, // força refresh
            BrokerAddressFamily = BrokerAddressFamily.V4 // força IPv4 apenas
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