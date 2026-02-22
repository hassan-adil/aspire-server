using Shared.Abstractions.Context;
using Shared.Abstractions.CQRS;
using Shared.Abstractions.Models;
using Shared.Kernel.Models;
using User.Application.CQRS.Users.DTOs;
using User.Application.CQRS.Users.Queries;
using User.Infrastructure.Services;

namespace User.Application.CQRS.Users.Handlers;

public class ReadAllUsersHandler(KeycloakService keycloakService, IRequestExecutionContext executionContext) : IQueryHandler<ReadAllUsersQuery, PaginatedList<UserListDto>>
{
    public async Task<Result<PaginatedList<UserListDto>>> Handle(ReadAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await keycloakService.ReadAllUsersAsync(request.Search, 
            request.Email, 
            request.UserName,
            request.Enabled, 
            request.Page, 
            request.PageSize);

            return Result.Success(new PaginatedList<UserListDto>(
                UserListDto.TransformAll(users.List)!,
                users.Count,
                request.Page,
                request.PageSize)); 
        }
        catch (Refit.ApiException ex)
        {
            executionContext.AddException(ex);

            return Result.Failure<PaginatedList<UserListDto>>(new Error("Keycloak.ExternalApi", "An error occurred while communicating with the Keycloak API. Exceptions logged."));
        }
        catch (Exception ex)
        {
            executionContext.AddException(ex);

            return Result.Failure<PaginatedList<UserListDto>>(new Error("User.General", "An error occurred while reading users. Exceptions logged."));
        }
    }
}
