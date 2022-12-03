var builder = WebApplication.CreateBuilder(args);

// Add TinyLogger, remember to disable the default Console logger in appsettings.json
builder.Logging.AddTinyConsoleLogger();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
