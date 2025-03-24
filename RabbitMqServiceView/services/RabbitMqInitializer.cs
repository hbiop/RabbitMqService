
using RabbitMQ.Client;
using RabbitMqService.App.Abstractions;

namespace RabbitMqServiceView.services
{
    public class RabbitMqInitializer : IHostedService
    {
        private readonly IChannelPool _channelPool;
        private readonly ILogger<RabbitMqInitializer> _logger;

        public RabbitMqInitializer(
            IChannelPool channelPool,
            ILogger<RabbitMqInitializer> logger)
        {
            _channelPool = channelPool;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var channel = await _channelPool.GetChannelAsync();

                await channel.ExchangeDeclareAsync(
                    exchange: "my_exchange",
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false
                );

                await channel.QueueDeclareAsync(
                    queue: "my_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                await channel.QueueBindAsync(
                    queue: "my_queue",
                    exchange: "my_exchange",
                    routingKey: "my_queue"
                );

                _channelPool.ReturnChannel(channel);

                _logger.LogInformation("Инфраструктура RabbitMQ успешно инициализирована.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка инициализации RabbitMQ.");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
