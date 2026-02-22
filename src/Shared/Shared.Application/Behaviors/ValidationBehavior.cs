using FluentValidation;
using MediatR;
using Shared.Abstractions.Dependencies;
using Shared.Abstractions.Models;
using Shared.Application.Behaviors.Attributes;
using System.Reflection;

namespace Shared.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> _validators) : IPipelineBehavior<TRequest, TResponse>, ITransientDependency
     where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> validators = _validators;

    // Cache Guid-marked properties per request type
    private static readonly PropertyInfo[] GuidProperties =
        [.. typeof(TRequest)
            .GetProperties()
            .Where(p =>
                p.PropertyType == typeof(string) &&
                Attribute.IsDefined(p, typeof(GuidValueAttribute)))];

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        // 1️⃣ FluentValidation
        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            failures.AddRange(
                validationResults
                    .Where(r => r.Errors.Count != 0)
                    .SelectMany(r => r.Errors));
        }

        // 2️⃣ Guid attribute validation
        foreach (var prop in GuidProperties)
        {
            var value = (string?)prop.GetValue(request);

            if (!Guid.TryParse(value, out _))
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    prop.Name,
                    $"'{prop.Name}' must be a valid GUID."));
            }
        }

        // 3️⃣ Return validation failure result
        if (failures.Count != 0)
        {
            var validationErrors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).ToArray());

            var validationError = Error.Validation(
                "Validation.Failed",
                "One or more validation errors occurred.",
                validationErrors);

            return CreateValidationFailureResult<TResponse>(validationError);
        }

        return await next(cancellationToken);
    }

    private static T CreateValidationFailureResult<T>(Error validationError)
    {
        // Result<T>
        if (typeof(T).IsGenericType &&
            typeof(T).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(T).GetGenericArguments()[0];
            var failureMethod = typeof(Result)
                .GetMethods()
                .First(m => m.Name == nameof(Result.Failure) &&
                            m.IsGenericMethodDefinition)
                .MakeGenericMethod(valueType);

            return (T)failureMethod.Invoke(null, new object[] { validationError })!;
        }

        // Result
        if (typeof(T) == typeof(Result))
        {
            return (T)(object)Result.Failure(validationError);
        }

        throw new InvalidOperationException(
            $"ValidationBehavior only supports Result or Result<T>. Found: {typeof(T).Name}");
    }
}
