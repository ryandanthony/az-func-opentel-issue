# az-func-opentel-issue

## Introduction

Repo to show issues with open telemetry and azure functions. 

## Docker Compose

This uses docker for all aspects of the spike.

Uses open telemetry collector + Jaeger + Prometheus + seq


### URLS

- [Jaeger UI](http://localhost:16686)
- [Zipkin](http://localhost:9411/)
- [Prometheus](http://localhost:9090/)
- [seq](http://localhost:8777/)

### Up

`docker-compose up -d && docker-compose logs -f --tail 100`

### Down

`docker-compose down -v`

## local.settings.json for Spike.Func

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "OTEL_SERVICE_NAME": "dev-spike-func",
    "OTEL_EXPORTER_OTLP_ENDPOINT": "http://localhost:4317/",
    "OTEL_EXPORTER_OTLP_PROTOCOL": "grpc",
    "ServiceBusConnection": "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"
  }
}
```
