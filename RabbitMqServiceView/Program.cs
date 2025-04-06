using NLog;
using NLog.Web;
using RabbitMqService.Setup;


var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    ServiceRegistration.AddApplicationServices(builder.Services, builder.Configuration);
    builder.Services.AddSwagger(); 

    var app = builder.Build();

    app.Environment.EnvironmentName = "Development";

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerSetup(); 
    }

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