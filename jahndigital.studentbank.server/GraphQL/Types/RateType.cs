using System;
using HotChocolate.Language;
using HotChocolate.Types;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.Types
{
    /// <summary>
    ///     US Currency as a float or string without the dollar symbol.
    /// </summary>
    public sealed class RateType : ScalarType
    {
        public RateType() : base("Rate")
        {
            Description = "API/Interest Rate represented as a float.  E.g. 0.02 is 0.02%";
        }

        // define which .NET type represents your type
        public override Type ClrType { get; } = typeof(Rate);

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
                    return Rate.FromRate(dec);
                }

                return null;
            }

            if (literal is FloatValueNode floatLiteral) {
                var dec = 0m;

                if (decimal.TryParse(floatLiteral.Value, out dec)) {
                    return Rate.FromRate(dec);
                }

                return null;
            }

            if (literal is NullValueNode) {
                return null;
            }

            throw new ArgumentException(
                "The Rate type can only parse string or float literals.",
                nameof(literal)
            );
        }

        // define how a native type is parsed into a literal,
        public override IValueNode ParseValue(object value)
        {
            if (value == null) {
                return new NullValueNode(null);
            }

            if (value is Rate s) {
                return new FloatValueNode(decimal.ToSingle(s.Value));
            }

            throw new ArgumentException(
                "The specified value has to be a Rate type in order to be parsed by the string type.");
        }

        // define the result serialization. A valid output must be of the following .NET types:
        // System.String, System.Char, System.Int16, System.Int32, System.Int64,
        // System.Float, System.Double, System.Decimal and System.Boolean
        public override object? Serialize(object value)
        {
            if (value == null) {
                return null;
            }

            if (value is Rate s) {
                return s.Value;
            }

            throw new ArgumentException("The specified value cannot be serialized by the RateType.");
        }

        // Try to deserialize the string into a Rate object
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

                value = Rate.FromRate(dec);

                return true;
            }

            if (serialized is float f) {
                value = Rate.FromRate((decimal) f);

                return true;
            }

            if (serialized is long l) {
                value = Rate.FromRate((decimal) l);

                return true;
            }

            if (serialized is decimal d) {
                value = Rate.FromRate(d);

                return true;
            }

            value = null;

            return false;
        }
    }
}