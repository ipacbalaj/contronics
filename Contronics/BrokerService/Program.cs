using BrokerService;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Logs info, warning, and errors
    .Enrich.FromLogContext()
    .Enrich.WithOpenTelemetrySpanId() 
    .Enrich.WithOpenTelemetryTraceId() // Adds TraceId and SpanId to logs
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();
builder.Host.UseSerilog();

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

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error is not null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception occurred.");
        }

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { Error = "An internal server error occurred." });
    });
});

app.MapPost("/api/sensor-data", async (SensorData data, IHubContext<SensorDataHub> hubContext, ILogger<Program> logger) =>
    {
        logger.LogInformation("Received sensor data: {SensorId}, {Value}, {Timestamp}", data.SensorId, data.Value, data.Timestamp);
        logger.LogWarning("This is a warning message.");
        if (string.IsNullOrEmpty(data.SensorId) || data.Timestamp == default)
        {
            return Results.BadRequest("Invalid sensor data.");
        }

        await Task.Delay(1000);
        logger.LogInformation("Sending sensor data to SignalR clients.");
        await hubContext.Clients.All.SendAsync("ReceiveSensorData", data.SensorId, data.Value, data.Timestamp);

        return Results.Ok(new { Status = "Sensor data received" });
    })
    .WithName("ReceiveSensorData")
    .WithOpenApi();


app.MapPost("/api/sensor-data-error", async (SensorData data, ILogger<Program> logger) =>
    {
        logger.LogInformation("Received sensor data: {SensorId}, {Value}, {Timestamp}", data.SensorId, data.Value, data.Timestamp);
        logger.LogWarning("This is a warning message before throwing an exception.");

        // Simulate an exception
        throw new InvalidOperationException("Simulated sensor data processing error.");

    })
    .WithName("ReceiveSensorDataError")
    .WithOpenApi();

app.Run();
