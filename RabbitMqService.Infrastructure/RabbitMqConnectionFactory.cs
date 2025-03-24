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
    /*public class RabbitMqConnectionFactory : App.Abstractions.IConnectionFactory, IDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly ConnectionFactory _rabbitFactory;
        private IConnection _connection;

        public RabbitMqConnectionFactory(IOptions<RabbitMqSettings> settings)
        {
            _rabbitFactory = new ConnectionFactory
            {
                Uri = new Uri(_settings.ConnectionString),
                AutomaticRecoveryEnabled = true // Автовосстановление
            };
        }

        public Task<IConnection> GetConnection()
        {
            if (_connection?.IsOpen == true)
                return _connection;

            // Логика повторных попыток
            int attempts = 0;
            while (attempts < _settings.MaxRetryAttempts)
            {
                try
                {
                    _connection?.Dispose();
                    _connection = await _rabbitFactory.CreateConnectionAsync();
                    return _connection;
                }
                catch (BrokerUnreachableException ex)
                {
                    if (++attempts >= _settings.MaxRetryAttempts)
                        throw new InvalidOperationException("Не удалось подключиться к RabbitMQ", ex);

                    Thread.Sleep(_settings.RetryDelayMs);
                }
            }
            return _connection;
        }

        public void Dispose() => _connection?.Close();
    }*/
}
