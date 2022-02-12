using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.Domain.Exceptions;

public class WithdrawalLimitExceededException : BaseException
{
    private Transaction? _transaction;

    public WithdrawalLimitExceededException(ShareType shareType, Share share) : base(
        $"Withdrawal limit exceeded on Share {share.Student?.AccountNumber ?? ""}#{share.Id}: Only {shareType.WithdrawalLimitCount} withdrawals are allowed per period."
    )
    {
        ShareType = shareType;
        Share = share;
    }

    public Share? Share { get; }

    public ShareType? ShareType { get; }

    /// <summary>
    ///     Get or set the transaction object representing the exception.
    /// </summary>
    public Transaction Transaction
    {
        get
        {
            if (_transaction == null)
            {
                _transaction = new Transaction
                {
                    Amount = Money.FromCurrency(0.0m),
                    NewBalance = Share?.Balance ?? Money.FromCurrency(0.0m),
                    TargetShare = Share ?? new Share { Id = -1 },
                    TargetShareId = Share?.Id ?? -1,
                    TransactionType = "EX",
                    EffectiveDate = DateTime.UtcNow,
                    Comment = Message
                };
            }

            return _transaction;
        }

        set => _transaction = value;
    }
}
