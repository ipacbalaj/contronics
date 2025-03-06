using InfoPointWeb;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);
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
            serviceName: "InfoPointWeb",
            serviceVersion: "1.0.0"))
        .AddAspNetCoreInstrumentation() // Automatically trace incoming HTTP requests
        .AddHttpClientInstrumentation() // Automatically trace outgoing HTTP requests
        .AddSource("InfoPoint   ActivitySource") // Add custom ActivitySource
        .AddSource("MassTransit") // MassTransit ActivitySource
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(x =>
{
    try
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var configuration = context.GetRequiredService<IConfiguration>();
            cfg.Host(configuration["RabbitMQ:HostName"], "/", h =>
            {
                h.Username(configuration["RabbitMQ:UserName"]);
                h.Password(configuration["RabbitMQ:Password"]);
            });

            cfg.ReceiveEndpoint("sensor-data-queue", e =>
            {
                e.Consumer<SensorDataConsumer>();
            });

        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warning] Database configuration failed: {ex.Message}");
    }

});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.Run();
