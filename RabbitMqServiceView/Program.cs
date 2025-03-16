using Microsoft.AspNetCore.Connections;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using RabbitMQ.Client;
using RabbitMqService.App.Abstractions;
using RabbitMqService.Domain.options;
using RabbitMqService.Infrastructure.RabbitMq;
using Serilog;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;


var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddHttpClient("RmqHttpClient", client =>
    {
        var rabbitAuth = builder.Configuration.GetSection("RabbitMQAuth"); 
        string _basicAuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{rabbitAuth["UserName"]}:{rabbitAuth["Password"]}"));
        string? connectionString = builder.Configuration.GetConnectionString("RabbitMQApi");
        client.BaseAddress = connectionString != null
        ? new Uri(connectionString)
        : throw new Exception("Строка подключения к Api RabbitMQ не найдена");
        client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", _basicAuthHeader);
    });
    builder.Services.AddSingleton<RabbitMQ.Client.IConnection>(serviceProvider =>
    {
        string? connectionString = builder.Configuration.GetConnectionString("RabbitMQ");
        var factory = new ConnectionFactory()
        {
            Uri = connectionString != null ? new Uri(connectionString) : throw new Exception("Строка подключения к RabbitMQ не найдена"),
            ClientProvidedName = "RabbitMqService"
        };
        return factory.CreateConnectionAsync().Result;
    });
    builder.Services.AddScoped<IProducer, RabbitMqProducer>();
    builder.Services.AddScoped<IConsumer, RabbitMqConsumer>();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    });


    var app = builder.Build();


    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
    app.MapControllers();
    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}