﻿using System;

namespace jahndigital.studentbank.utils
{
    /// <summary>
    /// Represents an immutable rate (APR, Dividend Rate, etc.)
    /// </summary>
    public class Rate: IComparable<decimal>, IComparable<Rate>
    {
        /// <summary>
        /// The precision when rounding and storing in the database.
        /// </summary>
        protected const int precision = 4;

        /// <summary>
        /// Represents the current value as a decimal.
        /// </summary>
        public decimal Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current value as a database long.
        /// </summary>
        public long DatabaseValue
        {
            get {
                var dPrecision = new decimal(Math.Pow(10, precision));
                var dAmount = decimal.Round(Value, precision, MidpointRounding.AwayFromZero);
                return decimal.ToInt64(decimal.Multiply(dAmount, dPrecision));
            }
        }

        /// <summary>
        /// Deserializes an integer value from the database.
        /// </summary>
        /// <param name="amount">The value from the database.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Rate FromDatabase(long amount)
        {
            var dAmount = new decimal(amount / Math.Pow(10, precision));
            return new Rate(dAmount);
        }

        /// <summary>
        /// Creates a new Rate object from the provided whole number.
        /// </summary>
        /// <param name="amount">The whole-number value.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Rate FromRate(long amount) => new Rate(new decimal(amount));

        /// <summary>
        /// Creates a new Rate object from the provided decmial number.
        /// </summary>
        /// <param name="amount">The decimal value.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Rate FromRate(decimal amount) => new Rate(amount);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Rate operator +(Rate a) => a;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rate operator +(Rate a, Rate b) =>
            new Rate(decimal.Add(a.Value, b.Value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Rate operator -(Rate a) =>
            new Rate(decimal.Negate(a.Value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rate operator -(Rate a, Rate b) =>
            new Rate(decimal.Subtract(a.Value, b.Value));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rate operator *(Rate a, Rate b) =>
            new Rate(decimal.Multiply(a.Value, b.Value));

        /// <summary>
        /// Determine if the first rate object is equal to the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Rate a, Rate b) => a.CompareTo(b) == 0;

        /// <summary>
        /// Determine if the first rate object is not equal to the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Rate a, Rate b) => a.CompareTo(b) != 0;

        /// <summary>
        /// Determine if the first rate object is greater than the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >(Rate a, Rate b) => a.CompareTo(b) == 1;

        /// <summary>
        /// Determine if the first rate object is less than the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <(Rate a, Rate b) => a.CompareTo(b) == -1;

        /// <summary>
        /// Determine if the first rate object is greater than or equal to the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator >=(Rate a, Rate b) => a.CompareTo(b) >= 1;

        /// <summary>
        /// Determine if the first rate object is less than or equal to the second.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator <=(Rate a, Rate b) => a.CompareTo(b) <= 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        protected Rate(decimal amount) => this.Value = amount;

        /// <summary>
        /// Outputs the money as a currency string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value.ToString("P");

        /// <summary>
        /// Compare the rate with another decimal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(decimal other)
        {
            if (Value > other) return 1;
            if (Value < other) return -1;
            return 0;
        }

        /// <summary>
        /// Compare two rate objects.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Rate other)
        {
            if (Value > other.Value) return 1;
            if (Value < other.Value) return -1;
            return 0;
        }

        /// <summary>
        /// Determine if the provided object equals this one.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is Rate r) {
                return CompareTo(r) == 0;
            }

            if (obj is decimal d) {
                return CompareTo(d) == 0;
            }

            return base.Equals(obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => base.GetHashCode();
    }
}
