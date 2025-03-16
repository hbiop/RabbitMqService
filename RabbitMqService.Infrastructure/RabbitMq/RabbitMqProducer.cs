using RabbitMQ.Client;
using RabbitMqService.App.Abstractions;
using RabbitMqService.Domain.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RabbitMqService.Infrastructure.RabbitMq
{
    public class RabbitMqProducer : IProducer
    {
        private IConnection _connection;
        public RabbitMqProducer(IConnection connection)
        {
            _connection = connection;
        }


        public async Task<string> SendMessage(PostMessageModel model)
        {
            var durable = model.Modifiers?.Contains("durable") ?? true;
            var exclusive = model.Modifiers?.Contains("exclusive") ?? false;
            var autoDelete = model.Modifiers?.Contains("auto_delete") ?? false;
            await using (var channel = await _connection.CreateChannelAsync())
            {
                await channel.ExchangeDeclareAsync(
                    exchange: "my_exchange",
                    type: ExchangeType.Direct,
                    durable: durable,
                    autoDelete: autoDelete,
                    arguments: null
                );

                await channel.QueueDeclareAsync(
                    queue: model.QueueName,
                    durable: durable,
                    exclusive: exclusive,
                    autoDelete: autoDelete,
                    arguments: null
                );
                await channel.QueueBindAsync(
                    queue: model.QueueName,
                    exchange: "my_exchange",
                    routingKey: model.QueueName
                );

                byte[] messageBodyBytes = JsonSerializer.SerializeToUtf8Bytes(model.Message);

                await channel.BasicPublishAsync(
                    exchange: "my_exchange",
                    routingKey: model.QueueName,
                    body: messageBodyBytes
                );

                return "Сообщение успешно отправлено.";
            }
        }
    }
}

