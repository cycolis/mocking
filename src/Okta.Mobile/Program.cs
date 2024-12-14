using Okta.DeviceAccess.Core.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddHttpClient();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();