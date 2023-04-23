using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(c =>
    {
        c.AddConsole();
        c.AddDebug();
        c.SetMinimumLevel(LogLevel.Information);
    }
);
builder.Services.AddControllers()
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
    });

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapControllers();

app.Run();