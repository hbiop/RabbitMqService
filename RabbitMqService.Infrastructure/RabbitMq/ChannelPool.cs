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
        private readonly IConnection _connection;
        private readonly int _poolSize;

        public ChannelPool(int poolSize, IConnection connection)
        {
            _poolSize = poolSize;
            _connection = connection;
            InitializePool();
        }

        private async void InitializePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                _channels.Enqueue(await _connection.CreateChannelAsync());
            }
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

    }
}
