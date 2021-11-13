#pragma warning disable CA1812 // Workaround for analyzer bug https://github.com/dotnet/roslyn-analyzers/issues/5628

var builder = WebApplication.CreateBuilder(args);

// Add TinyLogger, remember to disable the default Console logger in appsettings.json
builder.Logging.AddTinyConsoleLogger();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
