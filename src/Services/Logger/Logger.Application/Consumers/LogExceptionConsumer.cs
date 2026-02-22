using Logger.Core.Domain.Entities;
using Logger.Core.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Logging;

namespace Logger.Application.Consumers;

public class LogExceptionConsumer(IExceptionLogRepository exceptionLogRepository, ILogger<LogExceptionConsumer> logger) : IConsumer<LogException>
{
    public async Task Consume(ConsumeContext<LogException> context)
    {
        if (context is not null)
        {
            var fullMessage = context.Message;

            var entity = new ExceptionLog(fullMessage.ServiceName,
                fullMessage.TenantId,
                fullMessage.RequestPath,
                fullMessage.HttpMethod,
                fullMessage.RequestJson,
                fullMessage.TraceIdentifier,
                fullMessage.StatusCode,
                fullMessage.ErrorMessage,
                fullMessage.StackTrace,
                fullMessage.UserId,
                fullMessage.Timestamp,
                fullMessage.RuntimeMs);

            await exceptionLogRepository.InsertLog(entity);

            logger.LogInformation("ExceptionLog inserted successfully.");
        }

        return;
    }
}
