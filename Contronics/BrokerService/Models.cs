namespace BrokerService;

public record SensorDataMessage(string SensorId, double Value, DateTime Timestamp);
