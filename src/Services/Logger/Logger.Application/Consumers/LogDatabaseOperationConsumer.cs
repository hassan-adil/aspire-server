using Logger.Core.Domain.Entities;
using Logger.Core.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Logging;

namespace Logger.Application.Consumers;

public class LogDatabaseOperationConsumer(IDatabaseOperationLogRepository databaseOperationLogRepository, ILogger<LogDatabaseOperationConsumer> logger) : IConsumer<LogDatabaseOperation>
{
    public async Task Consume(ConsumeContext<LogDatabaseOperation> context)
    {
        if (context is not null)
        {
            var fullMessage = context.Message;

            var entity = new DatabaseOperationLog(fullMessage.ServiceName,
                fullMessage.TenantId,
                fullMessage.RequestPath,
                fullMessage.HttpMethod,
                fullMessage.TraceIdentifier,
                fullMessage.UserId,
                fullMessage.Action,
                fullMessage.EntityName,
                fullMessage.EntityId,
                fullMessage.SqlQuery,
                fullMessage.OldSnapshot,
                fullMessage.NewSnapshot,
                fullMessage.Timestamp,
                fullMessage.IsError,
                fullMessage.ErrorMessage,
                fullMessage.RuntimeMs,
                fullMessage.RowsAffected
                );

            await databaseOperationLogRepository.InsertLog(entity);

            logger.LogInformation("DatabaseOperationLog inserted successfully.");
        }

        return;
    }
}
