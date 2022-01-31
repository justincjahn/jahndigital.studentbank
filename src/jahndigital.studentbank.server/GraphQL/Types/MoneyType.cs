using System;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.Types
{
    /// <summary>
    /// Tells HotChocolate how to convert from/to the Money field into a native .NET object.
    /// </summary>
    public class MoneyType : ScalarType
    {
        /// <summary>
        /// The .net type representation of this scalar.
        /// </summary>
        public override Type RuntimeType { get; } = typeof(Money);

        public MoneyType() : this("Money") { }

        public MoneyType(NameString name, BindingBehavior bind = BindingBehavior.Explicit) : base(name, bind)
        {
            Description =
                "US Currency as a float (preferred) or string without the dollar symbol. E.g 10.33 is $10.33.";
        }

        /// <summary>
        /// Defines if the specified <paramref name="valueSyntax" />
        /// can be parsed by this scalar.
        /// </summary>
        /// <param name="valueSyntax">
        /// The literal that shall be checked.
        /// </param>
        /// <returns>
        /// <c>true</c> if the literal can be parsed by this scalar;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueSyntax" /> is <c>null</c>.
        /// </exception>
        public override bool IsInstanceOfType(IValueNode valueSyntax)
        {
            if (valueSyntax is null)
            {
                throw new ArgumentNullException(nameof(valueSyntax));
            }

            return valueSyntax is StringValueNode or NullValueNode or FloatValueNode;
        }

        /// <summary>
        /// Parses the specified <paramref name="valueSyntax" />
        /// to the .net representation of this type.
        /// </summary>
        /// <param name="valueSyntax">
        /// The literal that shall be parsed.
        /// </param>
        /// <param name="withDefaults">
        /// Can be ignored on leaf types.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="valueSyntax" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="SerializationException">
        /// The specified <paramref name="valueSyntax" /> cannot be parsed
        /// by this scalar.
        /// </exception>
        public override object? ParseLiteral(IValueNode valueSyntax, bool withDefaults = true)
        {
            if (valueSyntax is null)
            {
                throw new ArgumentNullException(nameof(valueSyntax));
            }

            if (valueSyntax is StringValueNode s)
            {
                if (decimal.TryParse(s.Value, out decimal dec))
                {
                    return Money.FromCurrency(dec);
                }
            }

            if (valueSyntax is IntValueNode i)
            {
                if (decimal.TryParse(i.Value, out decimal dec))
                {
                    return Money.FromCurrency(dec);
                }
            }

            if (valueSyntax is FloatValueNode f)
            {
                if (decimal.TryParse(f.Value, out decimal dec))
                {
                    return Money.FromCurrency(dec);
                }
            }

            if (valueSyntax is NullValueNode)
            {
                return null;
            }

            throw new SerializationException(
                "Unable to convert the provided value into a Money value. "
                + "The value must be in the form of a whole number, decimal or null.",
                this
            );
        }

        /// <summary>
        /// Parses the .net value representation to a value literal.
        /// </summary>
        /// <param name="runtimeValue">
        /// The .net value representation.
        /// </param>
        /// <returns>
        /// Returns a GraphQL literal representing the .net value.
        /// </returns>
        /// <exception cref="SerializationException">
        /// The specified <paramref name="runtimeValue" /> cannot be parsed
        /// by this scalar.
        /// </exception>
        public override IValueNode ParseValue(object? runtimeValue)
        {
            if (runtimeValue is null)
            {
                return NullValueNode.Default;
            }

            if (runtimeValue is Money m)
            {
                return new FloatValueNode(decimal.ToSingle(m.Amount));
            }

            throw new SerializationException(
                "Unable to convert the runtime value into a float or null.",
                this
            );
        }

        /// <summary>
        /// Parses a result value of this into a GraphQL value syntax representation.
        /// </summary>
        /// <param name="resultValue">
        /// A result value representation of this type.
        /// </param>
        /// <returns>
        /// Returns a GraphQL value syntax representation of the <paramref name="resultValue"/>.
        /// </returns>
        /// <exception cref="SerializationException">
        /// Unable to parse the given <paramref name="resultValue"/>
        /// into a GraphQL value syntax representation of this type.
        /// </exception>
        public override IValueNode ParseResult(object? resultValue)
        {
            return ParseValue(resultValue);
        }

        /// <summary>
        /// Tries to serializes the .net value representation to the output format.
        /// </summary>
        /// <param name="runtimeValue">
        /// The .net value representation.
        /// </param>
        /// <param name="resultValue">
        /// The serialized value.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value was correctly serialized; otherwise, <c>false</c>.
        /// </returns>
        public override bool TrySerialize(object? runtimeValue, out object? resultValue)
        {
            resultValue = null;

            if (runtimeValue is Money m)
            {
                resultValue = m.Amount;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to deserializes the value from the output format to the .net value representation.
        /// </summary>
        /// <param name="resultValue">
        /// The serialized value.
        /// </param>
        /// <param name="runtimeValue">
        /// The .net value representation.
        /// </param>
        /// <returns>
        /// <c>true</c> if the serialized value was correctly deserialized; otherwise, <c>false</c>.
        /// </returns>
        public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
        {
            runtimeValue = null;

            if (resultValue is null)
            {
                return true;
            }

            if (resultValue is string s)
            {
                if (decimal.TryParse(s, out decimal dec))
                {
                    runtimeValue = Money.FromCurrency(dec);

                    return true;
                }

                return false;
            }

            if (resultValue is int i)
            {
                runtimeValue = Money.FromCurrency((decimal)i);

                return true;
            }

            if (resultValue is float f)
            {
                runtimeValue = Money.FromCurrency((decimal)f);

                return true;
            }

            if (resultValue is long l)
            {
                runtimeValue = Money.FromCurrency(l);

                return true;
            }

            if (resultValue is decimal d)
            {
                runtimeValue = Money.FromCurrency(d);

                return true;
            }

            return false;
        }
    }
}
