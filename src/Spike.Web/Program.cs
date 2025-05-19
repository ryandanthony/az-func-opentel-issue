using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Flurl.Http;
using Microsoft.AspNetCore.HttpLogging;
using Spike.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration
    .AddInMemoryCollection(new List<KeyValuePair<string, string>>
    {
        new("OpenTel:Enable", "true"),
        new("OpenTel:Environment", "dev"),
        new("OpenTel:OtlpExporter:Endpoint", "http://localhost:4317/"),
        new("OpenTel:OtlpExporter:Protocol", "0"),
    }!);

builder.Host.SetupLogging();
builder.Services.SetupInstrumentation(builder.Configuration, "spike-web");
builder.Services.AddHttpLogging(o =>
{
    o.CombineLogs = true;
    o.LoggingFields = HttpLoggingFields.All;
});

// Add services to the container.
builder.Services.AddAuthorization();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.EnableTryItOutByDefault());
}
app.UseHttpLogging();
// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (HttpContext httpContext) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            })
        .ToArray();
    return forecast;
});


app.MapGet("/trigger-sb", async (HttpContext httpContext) =>
{
    var connectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
    var clientOptions = new ServiceBusClientOptions
    {
        //TransportType = ServiceBusTransportType.AmqpWebSockets
    };
    var client = new ServiceBusClient(connectionString, clientOptions);

    var sender = client.CreateSender("queue.in");

    try
    {
        await sender.SendMessageAsync(new ServiceBusMessage($"Message {Guid.NewGuid()} @ {DateTime.Now:f}"));
    }
    catch (Exception ex)
    {
        throw;
    }
    finally
    {
        await sender.DisposeAsync();
        await client.DisposeAsync();
    }

    return "ok";
});


app.MapGet("/trigger-blob", async (HttpContext httpContext) =>
{
    var blobContainerClient = new BlobContainerClient("UseDevelopmentStorage=true", "samples-workitems");
    blobContainerClient.CreateIfNotExists();
    
    var blobClient = blobContainerClient.GetBlobClient($"{Guid.NewGuid()}.json");

    string blobContents = $@"{{
'id':'{Guid.NewGuid()}'
}}";
    
    await blobClient.UploadAsync(BinaryData.FromString(blobContents), overwrite: true);
    return "ok";
});

app.Run();