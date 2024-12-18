using Okta.DeviceAccess.Core.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddHttpClient();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();