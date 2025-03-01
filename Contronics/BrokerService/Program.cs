using BrokerService;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapHub<SensorDataHub>("/sensorDataHub");

app.MapPost("/api/sensor-data", async (SensorData data, IHubContext<SensorDataHub> hubContext) =>
    {
        if (string.IsNullOrEmpty(data.SensorId) || data.Timestamp == default)
        {
            return Results.BadRequest("Invalid sensor data.");
        }
        await hubContext.Clients.All.SendAsync("ReceiveSensorData", data.SensorId, data.Value, data.Timestamp);

        return Results.Ok(new { Status = "Sensor data received" });
    })
    .WithName("ReceiveSensorData")
    .WithOpenApi();

app.Run();
