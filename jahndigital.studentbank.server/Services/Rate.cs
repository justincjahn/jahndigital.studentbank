using System;

namespace jahndigital.studentbank.server.Services
{
    /// <summary>
    /// Represents an immutable rate (APR, Dividend Rate, etc.)
    /// </summary>
    public class Rate
    {
        /// <summary>
        /// The precision when rouding and storing in the database.
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
            get
            {
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
        public static Rate FromRate(long amount)
        {
            return new Rate(new decimal(amount));
        }

        /// <summary>
        /// Creates a new Rate object from the provided decmial number.
        /// </summary>
        /// <param name="amount">The decimal value.</param>
        /// <returns>An immutable object that represents the currency from the database.</returns>
        public static Rate FromRate(decimal amount)
        {
            return new Rate(amount);
        }

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
        public static Rate operator +(Rate a, Rate b)
        {
            return new Rate(decimal.Add(a.Value, b.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Rate operator -(Rate a)
        {
            return new Rate(decimal.Negate(a.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rate operator -(Rate a, Rate b)
        {
            return new Rate(decimal.Subtract(a.Value, b.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Rate operator *(Rate a, Rate b)
        {
            return new Rate(decimal.Multiply(a.Value, b.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        protected Rate(decimal amount)
        {
            this.Value = amount;
        }

        /// <summary>
        /// Outputs the money as a currency string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.ToString("P");
        }
    }
}
