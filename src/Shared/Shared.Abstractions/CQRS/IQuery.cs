using MediatR;
using Shared.Abstractions.Models;

namespace Shared.Abstractions.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
