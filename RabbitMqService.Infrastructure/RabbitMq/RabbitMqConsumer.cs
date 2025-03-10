using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMqService.App.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.Infrastructure.RabbitMq
{
    public class RabbitMqConsumer : IConsumer
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IChannel _channel;
        public RabbitMqConsumer(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<string> GetMessage(string exchangeName, string routingKey, string queueName)
        {
            try
            {
                await using (_connection = await _connectionFactory.CreateConnectionAsync())
                {
                    await using (_channel = await _connection.CreateChannelAsync())
                    {
                        await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct);
                        await _channel.QueueDeclareAsync(queueName, false, false, false);
                        await _channel.QueueBindAsync(queueName, exchangeName, routingKey);
                        await _channel.BasicQosAsync(0, 1, false);
                        var consumer = new AsyncEventingBasicConsumer(_channel);
                        var eventHandler = new TaskCompletionSource<string>();
                        consumer.ReceivedAsync += async (ch, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            eventHandler.SetResult(Encoding.UTF8.GetString(body));
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        };
                        string consumerTag = await _channel.BasicConsumeAsync(queueName, false, consumer);
                        if (!eventHandler.Task.Wait(10000))
                        {
                            return "Время ожидания сообщения истекло.";
                        }
                        var result = await eventHandler.Task;
                        await _channel.BasicCancelAsync(consumerTag);
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                return "Произошла ошибка при отправке сообщения";
            }
        }
    }
}
