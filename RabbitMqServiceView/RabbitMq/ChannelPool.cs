using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMqService.App.Abstractions;
using RabbitMqService.Domain.settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService.Infrastructure.RabbitMq
{
    public class ChannelPool : IChannelPool
    {
        private readonly ConcurrentQueue<IChannel> _channels = new();
        private App.Abstractions.IConnectionFactory _connectionFactory;
        private readonly int _poolSize;
        private bool _isReconnecting = false;

        public ChannelPool(IOptions<RabbitMqSettings> settings, App.Abstractions.IConnectionFactory connectionFactory)
        {
            _poolSize = settings.Value.PoolSize;
            _connectionFactory = connectionFactory;
            InitializePool();
        }

        private async void InitializePool()
        {
            await ReconnectAsync();
        }

        public async Task<IChannel> GetChannelAsync()
        {
            if (_channels.TryDequeue(out var channel) && !channel.IsClosed)
            {
                return channel;
            }

            var connection = await _connectionFactory.GetConnection();
            return await connection.CreateChannelAsync();
        }

        public void ReturnChannel(IChannel channel)
        {
            if (channel.IsClosed)
            {
                channel.Dispose();
                return;
            }
            _channels.Enqueue(channel);
        }

        public async Task ReconnectAsync()
        {
            if (_isReconnecting) return;

            _isReconnecting = true;
            try
            {
                for (int i = 0; i < _poolSize; i++)
                {
                    var connection = await _connectionFactory.GetConnection();
                    _channels.Enqueue(await connection.CreateChannelAsync());
                }
                Console.WriteLine("Успешное переподключение к RabbitMQ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при попытке переподключения: {ex.Message}");
            }
            finally
            {
                _isReconnecting = false;
            }
        }
    }
}
