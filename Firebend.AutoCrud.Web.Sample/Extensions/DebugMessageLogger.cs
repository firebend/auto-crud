using System.Threading.Tasks;
using MassTransit.Audit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Web.Sample.Extensions;

public class DebugMessageLogger : IMessageAuditStore
{
    private readonly ILogger<DebugMessageLogger> _logger;

    public DebugMessageLogger(ILogger<DebugMessageLogger> logger)
    {
        _logger = logger;
    }

    public Task StoreMessage<T>(T message, MessageAuditMetadata metadata) where T : class
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return Task.CompletedTask;
        }

        _logger.LogDebug(
            "{Action} Message Bus {@Message} with {@Context} {@Payload}",
            metadata.ContextType, message, metadata, JsonConvert.SerializeObject(message, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

        return Task.CompletedTask;
    }
}
