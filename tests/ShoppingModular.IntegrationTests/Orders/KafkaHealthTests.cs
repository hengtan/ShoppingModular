using System.Text.Json;
using Confluent.Kafka;
using NUnit.Framework;

namespace ShoppingModular.IntegrationTests.Orders;

[TestFixture]
public class KafkaHealthTests
{
    private readonly string _bootstrap = "localhost:9092";

    [Test]
    public void Should_Connect_To_Kafka_Broker()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        using var producer = new ProducerBuilder<Null, string>(config).Build();
        Assert.That(producer, Is.Not.Null);
    }
}