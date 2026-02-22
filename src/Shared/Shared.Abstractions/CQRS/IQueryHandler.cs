using MediatR;
using Shared.Abstractions.Models;

namespace Shared.Abstractions.CQRS;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
