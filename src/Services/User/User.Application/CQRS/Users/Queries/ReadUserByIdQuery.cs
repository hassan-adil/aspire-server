using Shared.Abstractions.CQRS;
using Shared.Application.Behaviors.Attributes;
using User.Application.CQRS.Users.DTOs;

namespace User.Application.CQRS.Users.Queries;

public record ReadUserByIdQuery([property: GuidValue] string Id) : IQuery<UserDto>;