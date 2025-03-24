using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMqService.Domain.settings;
using System;
using System.Collections.Generic;
using RabbitMqService.App;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMqService.App.Abstractions;

namespace RabbitMqService.Infrastructure
{
    public class RabbitMqConnectionFactory : App.Abstractions.IConnectionFactory
    {
        private static IConnection _connection;
        private static readonly object _lock = new object();
        private readonly RabbitMqSettings _settings;
        ILogger<RabbitMqConnectionFactory> _logger;
        public RabbitMqConnectionFactory(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqConnectionFactory> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<IConnection> GetConnection()
        {
            if (_connection != null && _connection.IsOpen)
            {
                return _connection;
            }

            lock (_lock)
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    TryReconnectAsync().Wait();
                }

                return _connection;
            }
        }

        private async Task TryReconnectAsync()
        {
            int attempts = 0;

            while (attempts < _settings.MaxReconnectAttempts)
            {
                try
                {
                    var factory = new RabbitMQ.Client.ConnectionFactory()
                    {
                        HostName = _settings.HostName,
                        UserName = _settings.UserName,
                        Password = _settings.Password
                    };

                    _connection = await factory.CreateConnectionAsync();
                    break;
                }
                catch (Exception ex)
                {
                    attempts++;
                    if (attempts >= _settings.MaxReconnectAttempts)
                    {
                        _logger.LogCritical("Не удалось установить соединение после нескольких попыток.");
                        throw new Exception("Не удалось установить соединение после нескольких попыток.", ex);
                    }

                    await Task.Delay(_settings.ReconnectDelay);
                }
            }
        }
    }
}
