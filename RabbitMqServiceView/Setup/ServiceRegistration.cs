using RabbitMqService.App.Abstractions;
using RabbitMqService.Domain.settings;
using RabbitMqService.Infrastructure.RabbitMq;
using RabbitMqService.RabbitMq;
using RabbitMqService.settings;
using RabbitMqServiceView.services;

namespace RabbitMqService.Setup
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMqSettings"));
            services.Configure<RabbitMqApiSettings>(configuration.GetSection("RabbitMqApiSettings"));

            services.AddHttpClient<RmqHttpClient>();
            services.AddSingleton<IConnectionFactory, RabbitMqConnectionFactory>();
            services.AddScoped<IProducer, RabbitMqProducer>();
            services.AddScoped<IConsumer, RabbitMqConsumer>();
            services.AddSingleton<IChannelPool, ChannelPool>();
            services.AddHostedService<RabbitMqInitializer>();
        }
    }
}
