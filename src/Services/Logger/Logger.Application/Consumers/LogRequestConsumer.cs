using Logger.Core.Domain.Entities;
using Logger.Core.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Logging;

namespace Logger.Application.Consumers;

public class LogRequestConsumer(IRequestLogRepository requestLogRepository, ILogger<LogRequestConsumer> logger) : IConsumer<LogRequest>
{
    public async Task Consume(ConsumeContext<LogRequest> context)
    {
        if (context is not null)
        {
            var fullMessage = context.Message;

            var entity = new RequestLog(fullMessage.ServiceName, 
                fullMessage.TenantId, 
                fullMessage.RequestPath, 
                fullMessage.HttpMethod, 
                fullMessage.RequestJson, 
                fullMessage.ResponseJson, 
                fullMessage.StatusCode, 
                fullMessage.UserId, 
                fullMessage.Timestamp,
                fullMessage.RuntimeMs);

            await requestLogRepository.InsertLog(entity);

            logger.LogInformation("RequestLog inserted successfully.");
        }

        return;
    }
}
