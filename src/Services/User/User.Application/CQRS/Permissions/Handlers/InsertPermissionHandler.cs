using Shared.Abstractions.Context;
using Shared.Abstractions.CQRS;
using Shared.Abstractions.Models;
using Shared.Abstractions.UnitOfWork;
using Shared.Kernel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Application.CQRS.Permissions.Commands;
using User.Application.CQRS.Users.DTOs;
using User.Application.CQRS.Users.Queries;
using User.Core.Domain.Entities;
using User.Core.Repositories;
using User.Infrastructure.Services;

namespace User.Application.CQRS.Permissions.Handlers;

public class InsertPermissionHandler(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork, IRequestExecutionContext executionContext) : ICommandHandler<InsertPermissionCommand, string>
{
    public async Task<Result<string>> Handle(InsertPermissionCommand request, CancellationToken cancellationToken)
    {
        await permissionRepository.InsertAsync(new Permission()
        {
            Name = request.PermissionName,
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (executionContext.HasDatabaseOperations)
        {

        }

        return Result<string>.Success($"Permission {request.PermissionName} inserted.");
    }
}
