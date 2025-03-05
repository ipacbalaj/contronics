using System.Net.Http;
using System.Net.Http.Json;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
            serviceName: "ProxyService",
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

app.MapPost("/api/forward-sensor-data", async (HttpContext context, HttpClient httpClient, SensorData data) =>
    {
        // Define the existing sensor data API endpoint
        var sensorDataApiUrl = "http://localhost:5244/api/sensor-data"; // Replace with actual URL
        await Task.Delay(1500);
        if (string.IsNullOrEmpty(data.SensorId) || data.Timestamp == default)
        {
            return Results.BadRequest("Invalid sensor data.");
        }

        // Send HTTP POST request to the existing API
        var response = await httpClient.PostAsJsonAsync(sensorDataApiUrl, data);

        if (response.IsSuccessStatusCode)
        {
            return Results.Ok(new { Status = "Sensor data forwarded successfully" });
        }

        return Results.StatusCode((int)response.StatusCode);
    })
    .WithName("ForwardSensorData")
    .WithOpenApi();

app.MapPost("/api/forward-sensor-data-queue", async (HttpContext context, HttpClient httpClient, SensorData data) =>
    {
        // Define the existing sensor data API endpoint
        var sensorDataApiUrl = "http://localhost:5244/api/queue-demo/sensor-data"; // Replace with actual URL
        await Task.Delay(1500);
        if (string.IsNullOrEmpty(data.SensorId) || data.Timestamp == default)
        {
            return Results.BadRequest("Invalid sensor data.");
        }

        // Send HTTP POST request to the existing API
        var response = await httpClient.PostAsJsonAsync(sensorDataApiUrl, data);

        if (response.IsSuccessStatusCode)
        {
            return Results.Ok(new { Status = "Sensor data forwarded successfully" });
        }

        return Results.StatusCode((int)response.StatusCode);
    })
    .WithName("ForwardSensorDataQueue")
    .WithOpenApi();

app.Run();

public class SensorData
{
    public string SensorId { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}