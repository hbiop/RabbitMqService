using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
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
        private readonly ILogger<RabbitMqProducer> logger;

        public RabbitMqProducer(IChannelPool channelPool, ILogger<RabbitMqProducer> logger)
        {
            _channelPool = channelPool;
        }

        public async Task<string> SendMessage(PostMessageModel model)
        {
            var channel = await _channelPool.GetChannelAsync();

            try
            {
                byte[] messageBodyBytes = JsonSerializer.SerializeToUtf8Bytes(model.Message);
                var properties = new BasicProperties
                {
                    Persistent = model.Modifiers.persistent
                };

                await channel.BasicPublishAsync(
                    exchange: "my_exchange",
                    routingKey: model.QueueName,
                    body: messageBodyBytes,
                    mandatory:true,
                    basicProperties: properties
                );

                return "Сообщение успешно отправлено.";
            }
            catch (BrokerUnreachableException ex)
            {
                logger.LogError($"Ошибка соединения с RabbitMQ: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError($"Произошла ошибка: {ex.Message}");
                throw;
            }
            finally
            {
                _channelPool.ReturnChannel(channel);
            }
        }
    }
}

