using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Shared.Messaging
{
    public class RabbitMQConsumer : IMessageConsumer
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQConsumer>? _logger;
        private string? _consumerTag;

        public RabbitMQConsumer(string connectionString, ILogger<RabbitMQConsumer>? logger = null)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(connectionString)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public async Task StartConsumingAsync(string queueName, Func<NotificationMessage, Task> onMessageReceived)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var notification = JsonConvert.DeserializeObject<NotificationMessage>(message);

                    if (notification != null)
                    {
                        _logger?.LogInformation("Message received from queue: {Queue}", queueName);
                        await onMessageReceived(notification);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger?.LogWarning("Failed to deserialize message from queue");
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error processing message");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _consumerTag = _channel.BasicConsume(queueName, false, consumer);
            await Task.CompletedTask;
        }

        public Task StopAsync()
        {
            if (!string.IsNullOrEmpty(_consumerTag))
                _channel.BasicCancel(_consumerTag);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
