namespace Shared.Kernel.Abstractions.Domain;

public interface IEntity<T>
{
    T Id { get; }
}