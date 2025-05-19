using System.IO;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Spike.Func;

public class BlobFunction
{
    private readonly ILogger<BlobFunction> _logger;
    private readonly Instrumentation _instrumentation;

    public BlobFunction(ILogger<BlobFunction> logger, Instrumentation instrumentation)
    {
        _logger = logger;
        _instrumentation = instrumentation;
    }

    [Function(nameof(BlobFunction))]
    public async Task Run([BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")] Stream stream,
        string name)
    {
        using var activity = _instrumentation.ActivitySource.StartActivity("blob/BlobFunction");
        activity?.AddTag("a", "b");
        //^^ activity is null, but open tel is wired up to listen to it. See:   p.AddSource("Azure.*", Instrumentation.ActivitySourceName)

        using var blobStreamReader = new StreamReader(stream);
        var content = await blobStreamReader.ReadToEndAsync();
        _logger.LogInformation("C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}", name,
            content);

        var result = await "https://localhost:7053"
            .AppendPathSegment("weatherforecast")
            .GetJsonAsync<List<WeatherForecast>>();
        _logger.LogInformation("Count: {1}", result.Count);

    }
}