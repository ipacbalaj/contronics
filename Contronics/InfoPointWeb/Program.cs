using InfoPointWeb;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}