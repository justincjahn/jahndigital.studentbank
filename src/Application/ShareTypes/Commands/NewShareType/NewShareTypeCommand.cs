using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.ShareTypes.Commands.NewShareType;

/// <summary>
/// Create a new Share Type.
/// </summary>
public record NewShareTypeCommand : IRequest<long>
{
    public string Name { get; init; } = default!;
    public Rate DividendRate { get; init; } = Rate.Zero;
    public Period WithdrawalLimitPeriod { get; init; } = Period.Monthly;
    public int WithdrawalLimitCount { get; init; } = 0;
    public bool WithdrawalLimitShouldFee { get; init; } = false;
    public Money WithdrawalLimitFee { get; init; } = Money.Zero;
}

public class NewShareTypeCommandHandler : IRequestHandler<NewShareTypeCommand, long>
{
    private readonly IAppDbContext _context;

    public NewShareTypeCommandHandler(IAppDbContext context) {
        _context = context;
    }

    public async Task<long> Handle(NewShareTypeCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _context.ShareTypes.AnyAsync(x => x.Name == request.Name, cancellationToken);

        if (nameExists)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"A Share Type with the name {request.Name} already exists."
            );
        }

        var shareType = new ShareType
        {
            Name = request.Name,
            DividendRate = request.DividendRate,
            WithdrawalLimitCount = request.WithdrawalLimitCount,
            WithdrawalLimitPeriod = request.WithdrawalLimitPeriod,
            WithdrawalLimitShouldFee = request.WithdrawalLimitShouldFee,
            WithdrawalLimitFee = request.WithdrawalLimitFee
        };

        _context.ShareTypes.Add(shareType);
        await _context.SaveChangesAsync(cancellationToken);

        return shareType.Id;
    }
}
