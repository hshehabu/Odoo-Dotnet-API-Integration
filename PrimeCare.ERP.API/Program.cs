using Microsoft.OpenApi.Models;
using PrimeCare.ERP.API.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PrimeCare and Odoo Integration API", Version = "v1" });
    c.EnableAnnotations();  
});

// Odoo API configuration
var odooConfig = new OdooConfig
{
    ApiUrl = "http://localhost:8069",
    DbName = "dsp",
    UserName = "admin@example.com",
    Password = "admin"
};

var odooClient = new OdooApiClient(odooConfig);

// Login
var userId = await odooClient.LoginAsync();
if (userId == null)
{
    Console.WriteLine("Login failed.");
    return;
}
Console.WriteLine("Login successful. User ID: " + userId);

builder.Services.AddSingleton(new OdooApiClient(odooConfig));
builder.Services.AddTransient<EmployeeController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
