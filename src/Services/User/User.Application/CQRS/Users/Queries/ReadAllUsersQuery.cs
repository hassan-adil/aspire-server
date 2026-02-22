using Shared.Abstractions.CQRS;
using Shared.Kernel.Models;
using User.Application.CQRS.Users.DTOs;

namespace User.Application.CQRS.Users.Queries;

public record ReadAllUsersQuery(string? Search = null, 
    string? Email = null, 
    string? UserName = null, 
    bool Enabled = true,
    int Page = 1,
    int PageSize = 100) : IQuery<PaginatedList<UserListDto>>;
