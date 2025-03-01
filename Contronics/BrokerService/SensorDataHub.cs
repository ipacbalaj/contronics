using Microsoft.AspNetCore.SignalR;

namespace BrokerService;

public class SensorDataHub: Hub
{
    public async Task SendSensorData(SensorData data)
    {
        await Clients.All.SendAsync("ReceiveSensorData", data);
    }
}

public record SensorData(string SensorId, double Value, DateTime Timestamp)
{
}