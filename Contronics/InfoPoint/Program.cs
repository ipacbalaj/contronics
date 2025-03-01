using Microsoft.AspNetCore.SignalR.Client;

string hubUrl = "http://localhost:5244/sensorDataHub"; 

var connection = new HubConnectionBuilder()
    .WithUrl(hubUrl)
    .Build();

connection.On<string, double, DateTime>("ReceiveSensorData", (sensorId, value, timestamp) =>
{
    Console.WriteLine($"Sensor: {sensorId}, Value: {value}, Timestamp: {timestamp}");
});

async Task StartConnectionAsync()
{
    try
    {
        await connection.StartAsync();
        Console.WriteLine("Connected to SignalR hub.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error connecting to hub: {ex.Message}");
    }
}

await StartConnectionAsync();

// Keep the console app running
await Task.Delay(-1);