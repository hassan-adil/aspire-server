using Shared.Abstractions.Context;
using Shared.Abstractions.Dependencies;
using Shared.Abstractions.Models;

namespace Shared.Infrastructure.Context;

public class RequestExecutionContext : IRequestExecutionContext, IScopedDependency
{
    private readonly List<Error> _errors = [];
    private readonly List<Exception> _exceptions = [];
    private readonly List<DatabaseOperation> _databaseOperations = [];

    public bool HasFailure => _errors.Count > 0;
    public bool HasException => _exceptions.Count > 0;
    public bool HasDatabaseOperations => _databaseOperations.Count > 0;

    public IReadOnlyList<Error> Errors => _errors;
    public IReadOnlyList<Exception> Exceptions => _exceptions;
    public IReadOnlyList<DatabaseOperation> DatabaseOperations => _databaseOperations;

    public void AddError(Error error)
    {
        if (error == Error.None) return;
            _errors.Add(error);
    }

    public void AddException(Exception exception) => _exceptions.Add(exception);

    public void AddDatabaseOperation(DatabaseOperation operation) => _databaseOperations.Add(operation);
}