using System;
using System.Globalization;

namespace jahndigital.studentbank.utils
{
    /// <summary>
    ///     Represents an immutable monetary value.
    /// </summary>
    public class Money : IComparable<decimal>, IComparable<Money>
    {
        /// <summary>
        ///     The precision when rounding and storing in the database (should probably be 2 for us currency)
        /// </summary>
        protected const int precision = 2;

        /// <summary>
        /// </summary>
        /// <param name="amount"></param>
        protected Money(decimal amount)
        {
            Amount = amount;
        }

        /// <summary>
        ///     Represents the current value as a decimal.
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        ///     Gets the current value as a database long.
        /// </summary>
        public long DatabaseAmount
        {
            get {
                var dPrecision = new decimal(Math.Pow(10, precision));
                var dAmount = decimal.Round(Amount, precision, MidpointRounding.AwayFromZero);

                return decimal.ToInt64(decimal.Multiply(dAmount, dPrecision));
            }
        }

        /// <summary>
        ///     Compare the monetary amount with another decimal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(decimal other)
        {
            if (Amount > other) {
                return 1;
            }

            if (Amount < other) {
                return -1;
            }

            return 0;
        }

        /// <summary>
        ///     Compare two money objects.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Money? other)
        {
            if (other is null) {
                return -1;
            }

            if (Amount > other.Amount) {
                return 1;
            }

            if (Amount < other.Amount) {
                return -1;
            }

            return 0;
        }

        /// <summary>
        ///     Deserializes an integer value from the database.
        /// </summary>
        /// <param name="amount">The value from the database.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Money FromDatabase(long amount)
        {
            var dAmount = new decimal(amount / Math.Pow(10, precision));

            return new Money(dAmount);
        }

        /// <summary>
        ///     Creates a new Money object from the provided whole number.
        /// </summary>
        /// <param name="amount">The whole-number value.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Money FromCurrency(long amount)
        {
            return new(new decimal(amount));
        }

        /// <summary>
        ///     Creates a new Money object from the provided decmial number.
        /// </summary>
        /// <param name="amount">The decimal value.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Money FromCurrency(decimal amount)
        {
            return new(amount);
        }

        /// <summary>
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Money operator +(Money a)
        {
            return a;
        }

        /// <summary>
        ///     Add two money values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Money operator +(Money a, Money b)
        {
            return new(decimal.Add(a.Amount, b.Amount));
        }

        /// <summary>
        ///     Negate a money value.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Money operator -(Money a)
        {
            return new(decimal.Negate(a.Amount));
        }

        /// <summary>
        ///     Subtract two money values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Money operator -(Money a, Money b)
        {
            return new(decimal.Subtract(a.Amount, b.Amount));
        }

        /// <summary>
        ///     Multiply two money values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Money operator *(Money a, Money b)
        {
            return new(decimal.Multiply(a.Amount, b.Amount));
        }

        /// <summary>
        ///     Multiply a money value by a rate.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Money operator *(Money a, Rate b)
        {
            return new(decimal.Multiply(a.Amount, b.Value));
        }

        /// <summary>
        ///     Multiply a money value by a long or integer.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Money operator *(Money a, long b)
        {
            return new(decimal.Multiply(a.Amount, b));
        }

        /// <summary>
        ///     Determine if two money objects are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Money a, Money b)
        {
            return a.CompareTo(b) == 0;
        }

        /// <summary>
        ///     Determine if two money objects are not equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Money a, Money b)
        {
            return a.CompareTo(b) != 0;
        }

        /// <summary>
        ///     Determine if the first money object is greater than the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(Money a, Money b)
        {
            return a.CompareTo(b) == 1;
        }

        /// <summary>
        ///     Determine if the first money object is less than the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(Money a, Money b)
        {
            return a.CompareTo(b) == -1;
        }

        /// <summary>
        ///     Determine if the first money object is greater than or equal to the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >=(Money a, Money b)
        {
            return a.CompareTo(b) >= 1;
        }

        /// <summary>
        ///     Determines if the first money object is less than or equal to the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <=(Money a, Money b)
        {
            return a.CompareTo(b) <= 0;
        }

        /// <summary>
        ///     Outputs the money as a currency string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Amount.ToString("C", CultureInfo.CurrentCulture);
        }

        /// <summary>
        ///     Determine if two money objects are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is Money m) {
                return CompareTo(m) == 0;
            }

            if (obj is decimal d) {
                return CompareTo(d) == 0;
            }

            return base.Equals(obj);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}