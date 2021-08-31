using System;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.services.Exceptions
{
    public class NonsufficientFundsException : BaseException
    {
        private Transaction? _transaction;

        public NonsufficientFundsException(Share share, Money amount) : base(
            $"Nonsufficient funds on Share {share.Student?.AccountNumber ?? ""}#{share.Id} ({share.Balance}) to honor transaction amount of {amount}."
        )
        {
            Share = share;
            Amount = amount;
        }

        public Share? Share { get; }

        public Money Amount { get; }

        /// <summary>
        ///     Get or set the transaction object representing the exception.
        /// </summary>
        public Transaction Transaction
        {
            get {
                if (_transaction == null) {
                    _transaction = new Transaction {
                        Amount = Money.FromCurrency(0.0m),
                        NewBalance = Share?.Balance ?? Money.FromCurrency(0.0m),
                        TargetShare = Share ?? new Share {Id = -1},
                        TargetShareId = Share?.Id ?? -1,
                        TransactionType = "N",
                        EffectiveDate = DateTime.UtcNow,
                        Comment = Message
                    };
                }

                return _transaction;
            }

            set => _transaction = value;
        }
    }
}