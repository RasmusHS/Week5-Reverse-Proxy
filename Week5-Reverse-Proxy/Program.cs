using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Serilog
// Read Serilog configuration from appsettings.json and set up logging to the console and a file.
// Setup rolling file logging with a new log file created each day, and include timestamps in the log entries for better traceability.
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
                 .WriteTo.Console()
                 .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
});

var app = builder.Build();

app.MapReverseProxy();

app.Run();
