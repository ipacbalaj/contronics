using MassTransit;
using Shared;

namespace InfoPointWeb;

public class SensorDataConsumer: IConsumer<SensorDataMessage>
{
    public async Task Consume(ConsumeContext<SensorDataMessage> context)
    {
        var message = context.Message;
        await Task.CompletedTask; // Simulate async work if needed   
    }
}