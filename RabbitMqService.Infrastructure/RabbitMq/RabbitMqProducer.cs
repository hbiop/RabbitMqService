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
        private readonly IChannelPool _channelPool;

        public RabbitMqProducer(IChannelPool channelPool)
        {
            _channelPool = channelPool;
        }

        public async Task<string> SendMessage(PostMessageModel model)
        {
            var channel = await _channelPool.GetChannelAsync();

            try
            {
                byte[] messageBodyBytes = JsonSerializer.SerializeToUtf8Bytes(model.Message);

                await channel.BasicPublishAsync(
                    exchange: "my_exchange",
                    routingKey: model.QueueName,
                    body: messageBodyBytes
                );

                return "Сообщение успешно отправлено.";
            }
            finally
            {
                _channelPool.ReturnChannel(channel);
            }
        }
    }
}

