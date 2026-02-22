using Shared.Abstractions.Context;
using Shared.Abstractions.CQRS;
using Shared.Abstractions.Models;
using User.Application.CQRS.Users.DTOs;
using User.Application.CQRS.Users.Queries;
using User.Infrastructure.Services;

namespace User.Application.CQRS.Users.Handlers;

public class ReadUserByIdHandler(KeycloakService keycloakService, IRequestExecutionContext executionContext) : IQueryHandler<ReadUserByIdQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(ReadUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await keycloakService.ReadUserByIdAsync(request.Id);

            if (user != null)
            {
                return Result.Success((UserDto)user!);
            }

            return Result.Failure<UserDto>(new Error("User.NotFound", $"User with id: {request.Id} not found."));
        }
        catch (Refit.ApiException ex)
        {
            executionContext.AddException(ex);

            if (ex.Message.Contains("404"))
                return Result.Failure<UserDto>(new Error("User.NotFound", $"User with id: {request.Id} not found."));

            return Result.Failure<UserDto>(new Error("Keycloak.ExternalApi", "An error occurred while communicating with the Keycloak API. Exceptions logged."));
        }
        catch (Exception ex)
        {
            executionContext.AddException(ex);

            return Result.Failure<UserDto>(new Error("User.General", $"An error occurred while reading user against id: {request.Id}. Exceptions logged."));
        }
    }
}
