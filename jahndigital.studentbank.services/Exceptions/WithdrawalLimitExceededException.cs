using System;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.services.Exceptions
{
    public class WithdrawalLimitExceededException : BaseException {
        public dal.Entities.Share? Share {get; private set;}

        public dal.Entities.ShareType? ShareType { get; private set; }

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
                        TransactionType = "EX",
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

        public WithdrawalLimitExceededException(dal.Entities.ShareType shareType, dal.Entities.Share share) : base (
            $"Withdrawal limit exceeded on Share {share.Student?.AccountNumber ?? ""}#{share.Id}: Only {shareType.WithdrawalLimitCount} withdrawals are allowed per period."
        ) {
            ShareType = shareType;
            Share = share;
        }
    }
}
