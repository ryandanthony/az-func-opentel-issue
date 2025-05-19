using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Spike.Func;

public class ApiFunction
{
    private readonly ILogger<ApiFunction> _logger;
    private readonly Instrumentation _instrumentation;

    public ApiFunction(ILogger<ApiFunction> logger, Instrumentation instrumentation)
    {
        _logger = logger;
        _instrumentation = instrumentation;
    }

    [Function("weatherforecast")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        using var activity = _instrumentation.ActivitySource.StartActivity("httpTrigger/weatherforecast");
        activity?.AddTag("a", "b");
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var result = await "https://localhost:7215"
            .AppendPathSegment("weatherforecast")
            .GetJsonAsync<List<WeatherForecast>>();

        return new JsonResult(result);

    }
}