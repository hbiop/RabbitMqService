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
        private IConnection _connection;
        private static App.Abstractions.IConnectionFactory _connectionFactory;
        private readonly int _poolSize;
        private bool _isReconnecting = false;

        public ChannelPool(int poolSize, App.Abstractions.IConnectionFactory connectionFactory)
        {
            _poolSize = poolSize;
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

            return await _connection.CreateChannelAsync();
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
                if (_connection != null)
                {
                    _connection.Dispose();
                }

                _connection = await _connectionFactory.GetConnection();
                for (int i = 0; i < _poolSize; i++)
                {
                    _channels.Enqueue(await _connection.CreateChannelAsync());
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
