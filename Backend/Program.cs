using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddNewtonsoftJson(o => { o.SerializerSettings.TypeNameHandling = TypeNameHandling.All; });

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapControllers();

app.Run();