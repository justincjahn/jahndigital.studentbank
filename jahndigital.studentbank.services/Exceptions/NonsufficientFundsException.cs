using System;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.services.Exceptions
{
    public class NonsufficientFundsException : BaseException {
        public dal.Entities.Share? Share {get; private set;}

        public Money Amount { get; private set;}

        private dal.Entities.Transaction? _transaction;

        /// <summary>
        /// Get or set the transaction object representing the exception.
        /// </summary>
        public dal.Entities.Transaction Transaction {
            get {
                if (_transaction == null) {
                    _transaction = new dal.Entities.Transaction {
                        Amount = Money.FromCurrency(0.0m),
                        NewBalance = this.Share?.Balance ?? Money.FromCurrency(0.0m),
                        TargetShare = this.Share ?? new dal.Entities.Share() { Id = -1},
                        TargetShareId = this.Share?.Id ?? -1,
                        TransactionType = "NSF",
                        EffectiveDate = DateTime.UtcNow,
                        Comment = this.Message
                    };
                }

                return _transaction;
            }

            set {
                _transaction = value;
            }
        }

        public NonsufficientFundsException(dal.Entities.Share share, Money amount) : base (
            $"Nonsufficient funds on Share {share.Student?.AccountNumber ?? ""}#{share.Id} ({share.Balance}) to honor transaction amount of {amount}."
        ) {
            Share = share;
            Amount = amount;
        }
    }
}
