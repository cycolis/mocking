using Microsoft.EntityFrameworkCore;

using Okta.DeviceAccess.Core.Models;
using Okta.AuthServerApi.Data;
using Okta.AuthServerApi.Repositories;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Console.WriteLine($"Current Environment: {environment}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("MockDatabase"));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();