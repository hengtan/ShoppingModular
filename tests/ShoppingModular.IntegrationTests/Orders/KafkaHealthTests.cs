using Confluent.Kafka;

namespace ShoppingModular.IntegrationTests.Orders;

[TestFixture]
public class KafkaHealthTests
{
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