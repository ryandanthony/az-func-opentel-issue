
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Instrumentation.Runtime;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Exceptions;

namespace Spike.Web;

[ExcludeFromCodeCoverage]
public static class ConfigInstrumentation
{
    public static IHostBuilder SetupLogging(this IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseSerilog(
            (context, configuration) =>
            {
                var template = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
                configuration
                    .Enrich
                    .FromLogContext()
                    .Enrich
                    .WithExceptionDetails()
                    .WriteTo
                    .Console(outputTemplate: template);
            },
            writeToProviders: true);

        return builder;
    }

    public static void SetupInstrumentation(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<OtlpExporterOptions>(configuration.GetSection("OpenTel:OtlpExporter"));
        services.Configure<OpenTelemetryLoggerOptions>(configuration.GetSection("OpenTel:Logger"));
        services.Configure<HttpClientTraceInstrumentationOptions>(configuration.GetSection("OpenTel:Instrumentation:HttpClientTrace"));
        services.Configure<AspNetCoreTraceInstrumentationOptions>(configuration.GetSection("OpenTel:Instrumentation:AspNetCoreTrace"));
        services.Configure<RuntimeInstrumentationOptions>(configuration.GetSection("OpenTel:Instrumentation:Runtime"));

        var environment = configuration.GetValue<string>("OpenTel:Environment") ?? "unknown";
        var internalServiceName = $"{environment}-{serviceName}";
        const string serviceNamespace = "naf-link";
        var version = typeof(ConfigInstrumentation).Assembly?.GetName()?.Version?.ToString() ?? "unknown";
        var hostname = System.Net.Dns.GetHostName();

        services.AddOpenTelemetry()
            .ConfigureResource(p =>
            {
                p.AddService(
                        serviceName: internalServiceName,
                        serviceNamespace: serviceNamespace,
                        serviceVersion: version)
                    .AddAttributes(
                        [
                            new KeyValuePair<string, object>("hostname", hostname)
                        ]
                    );
            })
            .WithTracing(p =>
            {
                p.AddSource(Instrumentation.ActivitySourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation(options =>
                    {
                        //add filter to drop HTTP client activities that would be duplicates of Azure activities, since we captured them already with Azure.*
                        options.FilterHttpRequestMessage = _ =>
                            Activity.Current?.Parent?.Source?.Name != "Azure.Core.Http";
                    })
                    .AddOtlpExporter();
            })
            .WithMetrics(p =>
            {
                p.AddMeter(Instrumentation.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter();
            })
            .WithLogging(p =>
            {
                p.AddOtlpExporter();
            });
    }
}

