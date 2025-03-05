using MassTransit;
using Microsoft.Extensions.Logging;
using Serilog;
using Shared;

namespace InfoPointWeb;

public class SensorDataConsumer : IConsumer<SensorDataMessage>
{
    public async Task Consume(ConsumeContext<SensorDataMessage> context)
    {   
        Log.Information("Received sensor data message via queue: {Message}", context.Message);
        var message = context.Message;
        // Simulate async work if needed
        await Task.CompletedTask;  
    }
}