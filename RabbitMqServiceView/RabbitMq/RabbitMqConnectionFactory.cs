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
using RabbitMQ.Client.Events;

namespace RabbitMqService.RabbitMq
{
    public class RabbitMqConnectionFactory : App.Abstractions.IConnectionFactory, IDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqConnectionFactory> _logger;
        private IConnection? _connection;
        private readonly ConnectionFactory _factory;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        public RabbitMqConnectionFactory(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqConnectionFactory> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            _factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<IConnection> GetConnection()
        {
            if (_connection?.IsOpen == true)
                return _connection;

            await _connectionLock.WaitAsync();
            try
            {
                if (_connection?.IsOpen == true)
                    return _connection;

                return await InitializeConnectionAsync();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task<IConnection> InitializeConnectionAsync()
        {
            try
            {
                _connection?.Dispose();
                _connection = await _factory.CreateConnectionAsync();

                SubscribeToConnectionEvents();

                _logger.LogInformation("RabbitMQ connection established");
                return _connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to establish RabbitMQ connection");
                throw;
            }
        }

        private void SubscribeToConnectionEvents()
        {
            if (_connection != null)
            {
                _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
                _connection.ConnectionBlockedAsync += OnConnectionBlockedAsync;
                _connection.ConnectionUnblockedAsync += OnConnectionUnblockedAsync;
                _connection.CallbackExceptionAsync += OnCallbackExceptionAsync;
            }
        }

        private Task OnConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection shutdown detected. Initiator: {Initiator}", e.Initiator);
            return Task.CompletedTask; 
        }

        private Task OnConnectionBlockedAsync(object sender, ConnectionBlockedEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection blocked. Reason: {Reason}", e.Reason);
            return Task.CompletedTask; 
        }

        private Task OnConnectionUnblockedAsync(object sender, AsyncEventArgs e)
        {
            _logger.LogInformation("RabbitMQ connection unblocked");
            return Task.CompletedTask; 
        }

        private Task OnCallbackExceptionAsync(object sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, "RabbitMQ callback exception occurred");
            return Task.CompletedTask; 
        }

        public async Task<bool> TryManualRecoverAsync()
        {
            await _connectionLock.WaitAsync();
            try
            {
                await InitializeConnectionAsync();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public void Dispose()
        {
            _connectionLock.Wait();
            try
            {
                if (_connection != null)
                {
                    UnsubscribeFromConnectionEvents();

                    _connection.Dispose();
                    _connection = null;
                }
            }
            finally
            {
                _connectionLock.Release();
                _connectionLock.Dispose();
            }
        }

        private void UnsubscribeFromConnectionEvents()
        {
            if (_connection != null)
            {
                _connection.ConnectionShutdownAsync -= OnConnectionShutdownAsync;
                _connection.ConnectionBlockedAsync -= OnConnectionBlockedAsync;
                _connection.ConnectionUnblockedAsync -= OnConnectionUnblockedAsync;
                _connection.CallbackExceptionAsync -= OnCallbackExceptionAsync;
            }
        }
    }
}
