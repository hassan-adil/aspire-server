using Shared.Abstractions.CQRS;

namespace User.Application.CQRS.Permissions.Commands;

public record InsertPermissionCommand
(string PermissionName) : ICommand<string>;
