namespace Shared.Abstractions.Models;

public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new("__NONE__", string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");

    // Validation errors with detailed field-level errors
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

    public static implicit operator Result(Error error) => Result.Failure(error);

    public Result ToResult() => Result.Failure(this);

    // Factory method for validation errors
    public static Error Validation(string code, string description, Dictionary<string, string> fieldErrors)
    {
        return new Error(code, description)
        {
            ValidationErrors = fieldErrors.ToDictionary(
                kvp => kvp.Key,
                kvp => new[] { kvp.Value })
        };
    }

    // Factory method for validation errors with multiple messages per field
    public static Error Validation(string code, string description, Dictionary<string, string[]> fieldErrors)
    {
        return new Error(code, description)
        {
            ValidationErrors = fieldErrors
        };
    }

    // Factory method for general errors
    public static Error General(string code, string description) => new(code, description);

    // Factory method for not found errors
    public static Error NotFound(string code, string description) => new(code, description);

    // Factory method for conflict errors
    public static Error Conflict(string code, string description) => new(code, description);

    // Factory method for unauthorized errors
    public static Error Unauthorized(string code, string description) => new(code, description);
    
    public static Error ExternalApi(string code, string description) => new(code, description);
}
