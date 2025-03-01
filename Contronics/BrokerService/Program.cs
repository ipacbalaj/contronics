using BrokerService;
using Microsoft.AspNetCore.SignalR;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
            serviceName: "BrokerService",
            serviceVersion: "1.0.0"))
        .AddAspNetCoreInstrumentation() // Automatically trace incoming HTTP requests
        .AddHttpClientInstrumentation() // Automatically trace outgoing HTTP requests
        .AddSource("BrokerServiceActivitySource") // Add custom ActivitySource
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
// app.UseHttpsRedirection();
app.MapHub<SensorDataHub>("/sensorDataHub");

app.MapPost("/api/sensor-data", async (SensorData data, IHubContext<SensorDataHub> hubContext) =>
    {
        if (string.IsNullOrEmpty(data.SensorId) || data.Timestamp == default)
        {
            return Results.BadRequest("Invalid sensor data.");
        }

        await Task.Delay(1000);
        await hubContext.Clients.All.SendAsync("ReceiveSensorData", data.SensorId, data.Value, data.Timestamp);

        return Results.Ok(new { Status = "Sensor data received" });
    })
    .WithName("ReceiveSensorData")
    .WithOpenApi();

app.Run();
