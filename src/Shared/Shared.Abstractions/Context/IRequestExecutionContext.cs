using Shared.Abstractions.Models;

namespace Shared.Abstractions.Context;

public interface IRequestExecutionContext
{
    bool HasFailure { get; }
    bool HasException { get; }
    bool HasDatabaseOperations { get; }

    IReadOnlyList<Error> Errors { get; }
    IReadOnlyList<Exception> Exceptions { get; }
    IReadOnlyList<DatabaseOperation> DatabaseOperations { get; }

    void AddError(Error error);
    void AddException(Exception exception);
    void AddDatabaseOperation(DatabaseOperation operation);
}
