using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spike.Func;


var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddScoped<Instrumentation>();
builder.Configuration
    .AddInMemoryCollection(new List<KeyValuePair<string, string>>
    {
        new("OpenTel:Enable", "true"),
        new("OpenTel:Environment", "dev"),
        new("OpenTel:OtlpExporter:Endpoint", "http://localhost:4317/"),
        new("OpenTel:OtlpExporter:Protocol", "0"),
    }!);

builder.Services.SetupLogging();
builder.Services.SetupInstrumentation(builder.Configuration, "spike-func");

// Tells the host to no longer emit telemetry on behalf of the worker.
builder.Services.Configure<WorkerOptions>(workerOptions => workerOptions.Capabilities["WorkerOpenTelemetryEnabled"] = bool.TrueString);

builder.ConfigureFunctionsWebApplication();

var app = builder.Build();

app.Run();