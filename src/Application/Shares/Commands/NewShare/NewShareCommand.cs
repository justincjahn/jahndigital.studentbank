using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Shares.Commands.NewShare;

/// <summary>
/// Create a new share.
/// </summary>
/// <param name="StudentId"></param>
/// <param name="ShareTypeId"></param>
/// <param name="InitialBalance"></param>
public record NewShareCommand(long StudentId, long ShareTypeId, Money InitialBalance) : IRequest<long>;

public class NewShareCommandHandler : IRequestHandler<NewShareCommand, long>
{
    private IAppDbContext _context;

    public NewShareCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<long> Handle(NewShareCommand request, CancellationToken cancellationToken)
    {
        Student student = await _context.Students
            .Include(x => x.Group)
            .ThenInclude(x => x.Instance)
            .ThenInclude(x => x.ShareTypeInstances)
            .Where(x => x.Id == request.StudentId)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Student), request.StudentId);

        var hasShareType = student.Group.Instance.ShareTypeInstances.Any(x => x.ShareTypeId == request.ShareTypeId);

        if (!hasShareType)
        {
            throw new InvalidOperationException(
                $"Student {student.AccountNumber} in instance {student.Group.Instance.Description} does not have access to share type id {request.ShareTypeId}"
            );
        }

        var share = new Share
        {
            StudentId = request.StudentId,
            ShareTypeId = request.ShareTypeId,
            DateLastActive = DateTime.UtcNow,
            Balance = request.InitialBalance
        };

        _context.Shares.Add(share);
        await _context.SaveChangesAsync(cancellationToken);
        return share.Id;
    }
}
