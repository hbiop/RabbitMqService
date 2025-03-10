using RabbitMQ.Client;
using RabbitMqService.App.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.Infrastructure.RabbitMq
{
    public class RabbitMqProducer : IProducer
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IChannel _channel;
        public RabbitMqProducer(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        public async Task<string> SendMessage(string message, string exchangeName, string routingKey, string queueName)
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
                        byte[] messageBodyBytes = Encoding.UTF8.GetBytes(message);
                        await _channel.BasicPublishAsync(exchangeName, routingKey, messageBodyBytes);
                        await _channel.CloseAsync();
                        await _connection.CloseAsync();
                        return "Сообщение доставленно";
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
