using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.ShareTypes.Commands.UpdateShareType;

public record UpdateShareTypeCommand : IRequest
{
    public long ShareTypeId { get; init; }
    public string? Name { get; init; }
    public Rate? DividendRate { get; init; }
    public Period? WithdrawalLimitPeriod { get; init; }
    public int? WithdrawalLimitCount { get; init; }
    public bool? WithdrawalLimitShouldFee { get; init; }
    public Money? WithdrawalLimitFee { get; init; }
}

public class UpdateShareTypeCommandHandler : IRequestHandler<UpdateShareTypeCommand>
{
    private readonly IAppDbContext _context;

    public UpdateShareTypeCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateShareTypeCommand request, CancellationToken cancellationToken)
    {
        var shareType = await _context.ShareTypes.FindAsync(new object?[] { request.ShareTypeId }, cancellationToken)
            ?? throw new NotFoundException(nameof(ShareType), request.ShareTypeId);

        if (request.Name is not null && request.Name != shareType.Name)
        {
            var nameExists = await _context.ShareTypes.Where(x => x.Name == request.Name).AnyAsync(cancellationToken);

            if (nameExists)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request),
             $"A Share Type with the name '{request.Name}' already exists."
                );
            }

            shareType.Name = request.Name;
        }

        shareType.DividendRate = request.DividendRate ?? shareType.DividendRate;
        shareType.WithdrawalLimitCount = request.WithdrawalLimitCount ?? shareType.WithdrawalLimitCount;
        shareType.WithdrawalLimitPeriod = request.WithdrawalLimitPeriod ?? shareType.WithdrawalLimitPeriod;
        shareType.WithdrawalLimitShouldFee = request.WithdrawalLimitShouldFee ?? shareType.WithdrawalLimitShouldFee;
        shareType.WithdrawalLimitFee = request.WithdrawalLimitFee ?? shareType.WithdrawalLimitFee;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
