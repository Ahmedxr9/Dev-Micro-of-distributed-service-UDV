using System;
using System.Threading.Tasks;

namespace Shared.Messaging
{
    public interface IMessageConsumer : IDisposable
    {
        Task StartConsumingAsync(string queueName, Func<NotificationMessage, Task> handler);
        Task StopAsync();
    }
}
