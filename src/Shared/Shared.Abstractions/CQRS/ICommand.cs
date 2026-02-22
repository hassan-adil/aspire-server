using MediatR;
using Shared.Abstractions.Models;

namespace Shared.Abstractions.CQRS;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}