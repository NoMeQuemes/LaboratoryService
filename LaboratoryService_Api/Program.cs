using LaboratoryService_Api.Data;
using LaboratoryService_Api.Utilities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ConvertHL7>(); // Acá se agregan el archivo para convertir a HL7
builder.Services.AddSingleton<TcpManager>(new TcpManager("181.104.119.82", 4000));
//builder.Services.AddSingleton<TcpManager>(new TcpManager("127.0.0.1", 22222));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DBHtest"));
});

var app = builder.Build();

// Resolver el servicio para inicializarlo al inicio
var tcpManager = app.Services.GetRequiredService<TcpManager>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
