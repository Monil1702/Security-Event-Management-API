using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SecurityEventManagement.Api.Data;
using SecurityEventManagement.Api.Middleware;
using SecurityEventManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SecurityDatabase");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'SecurityDatabase' is required.");
}

builder.Services.AddDbContext<SecurityDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ISecurityEventService, SecurityEventService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SecurityDbContext>();
    await SeedData.InitializeAsync(db, CancellationToken.None);
}

await app.RunAsync();

public partial class Program;
