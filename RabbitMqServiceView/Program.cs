using Microsoft.AspNetCore.Connections;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using RabbitMqService.App.Abstractions;
using RabbitMqService.Infrastructure.RabbitMq;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddSingleton<RabbitMQ.Client.IConnectionFactory>(serviceProvider =>
{
    string? connectionString = builder.Configuration.GetConnectionString("RabbitMQ");
    var factory = new ConnectionFactory()
    {
        Uri = connectionString != null ? new Uri(connectionString) : throw new Exception("Строка подключения к RabbitMQ не найдена"),
        ClientProvidedName = "RabbitMqService"
    };
    return factory;
});
builder.Services.AddScoped<IProducer, RabbitMqProducer>();
builder.Services.AddScoped<IConsumer, RabbitMqConsumer>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() 
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}"
    )
    .CreateLogger();
try
{
    Log.Information("Starting RabbitMqService");
}
catch (Exception e)
{
    Log.Fatal(e.Message);
    throw;
}

var app = builder.Build();


app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});
app.MapControllers();
app.Run();