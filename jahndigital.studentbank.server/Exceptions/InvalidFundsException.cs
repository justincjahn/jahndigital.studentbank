using System;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.Exceptions
{
    public class NonsufficientFundsException : BaseException {
        public dal.Entities.Share? Share {get; private set;}

        public Money Amount { get; private set;}

        public NonsufficientFundsException(dal.Entities.Share share, Money amount) : base (
            $"Nonsufficient funds on Share #{share.Id} ({share.Balance}) to honor transaction amount of {amount}."
        ) {
            Share = share;
            Amount = amount;
        }
    }
}
