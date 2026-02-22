namespace Shared.Abstractions.Models;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error state", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public override string ToString() =>
        IsSuccess ? "Success" : $"Failure: {Error}";
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public TValue? ValueOrDefault => _value;

    public static explicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public bool TryGetValue(out TValue? value)
    {
        value = _value;
        return IsSuccess;
    }

    public void Deconstruct(out bool isSuccess, out TValue? value, out Error error)
    {
        isSuccess = IsSuccess;
        value = _value;
        error = Error;
    }

    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(Error);
    }

    public override string ToString() =>
        IsSuccess ? $"Success: {_value}" : $"Failure: {Error}";
}
