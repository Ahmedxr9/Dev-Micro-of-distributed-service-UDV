using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Shared.Messaging;

public class RabbitMQProducer : IMessageProducer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQProducer>? _logger;

    public RabbitMQProducer(string connectionString, ILogger<RabbitMQProducer>? logger = null)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Declare dead letter exchange
        _channel.ExchangeDeclare(QueueNames.DeadLetterExchange, ExchangeType.Topic, durable: true);
        
        // Declare queues with dead letter exchange
        DeclareQueue(QueueNames.Notifications);
        DeclareQueue(QueueNames.Email);
        DeclareQueue(QueueNames.Sms);
        DeclareQueue(QueueNames.Push);
    }

    private void DeclareQueue(string queueName)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", QueueNames.DeadLetterExchange }
            }
        );
    }

    public async Task PublishAsync<T>(T message, string queueName, string? routingKey = null) where T : class
    {
        try
        {
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body
            );

            _logger?.LogInformation("Message published to queue: {QueueName}, MessageId: {MessageId}", 
                queueName, properties.MessageId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error publishing message to queue: {QueueName}", queueName);
            throw;
        }

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}

