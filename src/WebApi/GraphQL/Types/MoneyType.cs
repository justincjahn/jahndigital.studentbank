using System;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Types;

/// <summary>
/// Tells HotChocolate how to convert from/to the Money field into a native .NET object.
/// </summary>
public sealed class MoneyType : ScalarType
{
    public override Type RuntimeType { get; } = typeof(Money);

    public MoneyType() : this("Money") { }

    private MoneyType(NameString name, BindingBehavior bind = BindingBehavior.Explicit) : base(name, bind)
    {
        Description =
            "US Currency as a float (preferred) or string without the dollar symbol. E.g 10.33 is $10.33.";
    }

    /// <inheritdoc cref="ScalarType.IsInstanceOfType(HotChocolate.Language.IValueNode)"/>
    /// <remarks>This type supports null, string, or float values.</remarks>
    public override bool IsInstanceOfType(IValueNode valueSyntax)
    {
        if (valueSyntax is null)
        {
            throw new ArgumentNullException(nameof(valueSyntax));
        }

        return valueSyntax is StringValueNode or NullValueNode or FloatValueNode;
    }

    /// <inheritdoc cref="ScalarType.ParseLiteral"/>
    public override object? ParseLiteral(IValueNode valueSyntax)
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

    /// <inheritdoc cref="ScalarType.ParseValue"/>
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

    /// <inheritdoc cref="ScalarType.ParseResult"/>
    public override IValueNode ParseResult(object? resultValue)
    {
        return ParseValue(resultValue);
    }

    /// <inheritdoc cref="ScalarType.TrySerialize"/>
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

    /// <inheritdoc cref="ScalarType.TryDeserialize"/>
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
