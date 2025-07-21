using CFA.Clientes.Application.Interfaces;
using CFA.Clientes.Infrastructure.Data;
using CFA.Clientes.Infrastructure.Repositories;
using CFA.Clientes.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// 🟢 Configuración de la cadena de conexión (appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🟢 Agregar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 🟢 Inyección de dependencias
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// 🟢 Add Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        opt.JsonSerializerOptions.WriteIndented = true;
    });

// 🟢 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🟢 Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
