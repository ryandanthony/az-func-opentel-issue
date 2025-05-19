using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace Spike.Web;

[ExcludeFromCodeCoverage]
public sealed class Instrumentation : IDisposable
{
    private readonly Meter _meter;
    public const string ActivitySourceName = "Spike.Web";
    public const string MeterName = "Spike.Web";

    public Instrumentation()
    {
        var version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
        ActivitySource = new ActivitySource(ActivitySourceName, version);
        _meter = new Meter(MeterName, version);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}