using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Spike.Func;

public class ServiceBusFunctions(ILogger<ServiceBusFunctions> logger, Instrumentation instrumentation)
{
    [Function(nameof(ServiceBusReceivedMessageFunction))]
    [ServiceBusOutput("queue.out", Connection = "ServiceBusConnection")]
    public string ServiceBusReceivedMessageFunction(
        [ServiceBusTrigger("queue.in", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("sb/ServiceBusReceivedMessageFunction");
        activity?.AddTag("a", "b");

        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        var outputMessage = $"Output message created at {DateTime.Now}";
        return outputMessage;
    }

    [Function(nameof(ServiceBusReceivedMessageFunction2))]
    public void ServiceBusReceivedMessageFunction2(
        [ServiceBusTrigger("queue.out", Connection = "ServiceBusConnection")] ServiceBusReceivedMessage message)
    {
        using var activity = instrumentation.ActivitySource.StartActivity("sb/ServiceBusReceivedMessageFunction2");
        activity?.AddTag("a", "b");

        logger.LogInformation("Message ID: {id}", message.MessageId);
        logger.LogInformation("Message Body: {body}", message.Body);
        logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
    }
}