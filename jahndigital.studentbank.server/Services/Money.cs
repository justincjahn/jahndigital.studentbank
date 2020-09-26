using System;
using System.Globalization;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Represents an immutable monetary value.
    /// </summary>
    public class Money
    {
        /// <summary>
        /// The precision when rounding and storing in the database (should probably be 2 for us currency)
        /// </summary>
        protected const int precision = 2;

        /// <summary>
        /// Represents the current value as a decimal.
        /// </summary>
        public decimal Amount
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current value as a database long.
        /// </summary>
        public long DatabaseAmount
        {
            get
            {
                var dPrecision = new decimal(Math.Pow(10, precision));
                var dAmount = decimal.Round(Amount, precision, MidpointRounding.AwayFromZero);
                return decimal.ToInt64(decimal.Multiply(dAmount, dPrecision));
            }
        }

        /// <summary>
        /// Deserializes an integer value from the database.
        /// </summary>
        /// <param name="amount">The value from the database.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Money FromDatabase(long amount)
        {
            var dAmount = new decimal(amount / Math.Pow(10, precision));
            return new Money(dAmount);
        }

        /// <summary>
        /// Creates a new Money object from the provided whole number.
        /// </summary>
        /// <param name="amount">The whole-number value.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Money FromCurrency(long amount)
        {
            return new Money(new decimal(amount));
        }

        /// <summary>
        /// Creates a new Money object from the provided decmial number.
        /// </summary>
        /// <param name="amount">The decimal value.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Money FromCurrency(decimal amount)
        {
            return new Money(amount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Money operator +(Money a) => a;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Money operator +(Money a, Money b)
        {
            return new Money(decimal.Add(a.Amount, b.Amount));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Money operator -(Money a)
        {
            return new Money(decimal.Negate(a.Amount));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Money operator -(Money a, Money b)
        {
            return new Money(decimal.Subtract(a.Amount, b.Amount));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Money operator *(Money a, Money b)
        {
            return new Money(decimal.Multiply(a.Amount, b.Amount));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Money operator *(Money a, Rate b)
        {
            return new Money(decimal.Multiply(a.Amount, b.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        protected Money(decimal amount)
        {
            this.Amount = amount;
        }

        /// <summary>
        /// Outputs the money as a currency string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Amount.ToString("C", CultureInfo.CurrentCulture);
        }
    }
}
