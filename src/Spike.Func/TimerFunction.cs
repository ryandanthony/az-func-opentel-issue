using System.Diagnostics;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Spike.Func;

public class TimerFunction(ILoggerFactory loggerFactory, Instrumentation instrumentation)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TimerFunction>();

    [Function("TimerFunction")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("timer/TimerFunction");
        activity?.AddTag("a", "b");
        //^^ activity is null, but open tel is wired up to listen to it. See:   p.AddSource("Azure.*", Instrumentation.ActivitySourceName)


        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }

        var result = await "https://localhost:7215"
            .AppendPathSegment("weatherforecast")
            .GetJsonAsync<List<WeatherForecast>>();
        _logger.LogInformation("Count: {1}", result.Count);
    }
}