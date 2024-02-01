using TransformerBeeClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddTransient<ICanConvertToBo4e, TransformerBeeRestClient>();
builder.Services.AddTransient<ICanConvertToEdifact, TransformerBeeRestClient>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.Run();
