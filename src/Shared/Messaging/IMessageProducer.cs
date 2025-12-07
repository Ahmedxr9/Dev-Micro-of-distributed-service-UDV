namespace Shared.Messaging;

public interface IMessageProducer
{
    Task PublishAsync<T>(T message, string queueName, string? routingKey = null) where T : class;
}

