using LaboratoryService_Api.Data;
using LaboratoryService_Api.Utilities;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

// Inicializa NLog y crea logger global
//var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

var logger = NLog.LogManager.Setup()
    .LoadConfigurationFromFile("NLog.config")
    .GetCurrentClassLogger();

try
{
    logger.Debug("Iniciando aplicación...");

    var builder = WebApplication.CreateBuilder(args);

    // Configura NLog como proveedor de logging
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // Agregar servicios
    builder.Services.AddScoped<ConvertHL7>(); // conversor HL7
    builder.Services.AddSingleton<TcpClient>(new TcpClient("181.104.119.82", 4000));
    //builder.Services.AddSingleton<TcpClient>(new TcpClient("127.0.0.1", 5000));
    builder.Services.AddSingleton<TcpServer>(new TcpServer(5000));
    // builder.Services.AddSingleton<TcpManager>(new TcpManager("127.0.0.1", 22222));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<ApplicationDbContext>(option =>
    {
        option.UseSqlServer(builder.Configuration.GetConnectionString("DBHtest"));
    });

    var app = builder.Build();

    // Inicialización de servicios manuales (fuera del middleware)
    var tcpSender = app.Services.GetRequiredService<TcpClient>();
    var tcpServer = app.Services.GetRequiredService<TcpServer>();
    tcpServer.StartListening();

    // Configuración del middleware
    //if (app.Environment.IsDevelopment())
    //{
    //}
    app.UseSwagger();
    app.UseSwaggerUI();

    //app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    // Captura cualquier error de arranque
    logger.Error(ex, "Se produjo una excepción al iniciar la aplicación.");
    throw;
}
finally
{
    // Limpia recursos del logger
    LogManager.Shutdown();
}
