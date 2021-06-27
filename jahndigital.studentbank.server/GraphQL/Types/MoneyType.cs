using System;
using HotChocolate.Language;
using HotChocolate.Types;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.Types
{
    /// <summary>
    ///     US Currency as a float or string without the dollar symbol.
    /// </summary>
    public sealed class MoneyType : ScalarType
    {
        public MoneyType() : base("Money")
        {
            Description =
                "US Currency as a float (preferred) or string without the dollar symbol. E.g 10.33 is $10.33.";
        }

        // define which .NET type represents your type
        public override Type ClrType { get; } = typeof(Money);

        // define which literals this type can be parsed from.
        public override bool IsInstanceOfType(IValueNode literal)
        {
            if (literal == null) {
                throw new ArgumentNullException(nameof(literal));
            }

            return literal is StringValueNode
                || literal is NullValueNode
                || literal is FloatValueNode;
        }

        // define how a literal is parsed to the native .NET type.
        public override object? ParseLiteral(IValueNode literal)
        {
            if (literal == null) {
                throw new ArgumentNullException(nameof(literal));
            }

            if (literal is StringValueNode stringLiteral) {
                var dec = 0m;

                if (decimal.TryParse(stringLiteral.Value, out dec)) {
                    return Money.FromCurrency(dec);
                }

                return null;
            }

            if (literal is FloatValueNode floatLiteral) {
                var dec = 0m;

                if (decimal.TryParse(floatLiteral.Value, out dec)) {
                    return Money.FromCurrency(dec);
                }

                return null;
            }

            if (literal is NullValueNode) {
                return null;
            }

            throw new ArgumentException(
                "The Money type can only parse string or float literals.",
                nameof(literal)
            );
        }

        // define how a native type is parsed into a literal,
        public override IValueNode ParseValue(object value)
        {
            if (value == null) {
                return new NullValueNode(null);
            }

            if (value is Money s) {
                return new FloatValueNode(decimal.ToSingle(s.Amount));
            }

            throw new ArgumentException(
                "The specified value has to be a string or char in order to be parsed by the string type.");
        }

        // define the result serialization. A valid output must be of the following .NET types:
        // System.String, System.Char, System.Int16, System.Int32, System.Int64,
        // System.Float, System.Double, System.Decimal and System.Boolean
        public override object? Serialize(object value)
        {
            if (value == null) {
                return null;
            }

            if (value is Money s) {
                return s.Amount;
            }

            throw new ArgumentException("The specified value cannot be serialized by the MoneyType.");
        }

        // Try to deserialize the string into a Money object
        public override bool TryDeserialize(object serialized, out object? value)
        {
            if (serialized is null) {
                value = null;

                return true;
            }

            if (serialized is string s) {
                var dec = 0m;

                if (!decimal.TryParse(s, out dec)) {
                    value = null;

                    return false;
                }

                value = Money.FromCurrency(dec);

                return true;
            }

            if (serialized is float f) {
                value = Money.FromCurrency((decimal) f);

                return true;
            }

            if (serialized is long l) {
                value = Money.FromCurrency(l);

                return true;
            }

            if (serialized is decimal d) {
                value = Money.FromCurrency(d);

                return true;
            }

            value = null;

            return false;
        }
    }
}