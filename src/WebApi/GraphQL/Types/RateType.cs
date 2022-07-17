using System;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Types;

/// <summary>
/// Tells HotChocolate how to convert from/to the Rate field into a native .NET object.
/// </summary>
public sealed class RateType : ScalarType
{
    /// <inheritdoc cref="ScalarType.RuntimeType"/>
    public override Type RuntimeType { get; } = typeof(Rate);

    public RateType() : this("Rate") { }

    private RateType(NameString name, BindingBehavior bind = BindingBehavior.Explicit) : base(name, bind)
    {
        Description = "API/Interest Rate represented as a float.  E.g. 0.02 is 0.02%";
    }

    /// <inheritdoc cref="ScalarType.IsInstanceOfType(HotChocolate.Language.IValueNode)"/>
    public override bool IsInstanceOfType(IValueNode valueSyntax)
    {
        if (valueSyntax is null)
        {
            throw new ArgumentNullException(nameof(valueSyntax));
        }

        return valueSyntax is StringValueNode or FloatValueNode or NullValueNode;
    }

    /// <inheritdoc cref="ScalarType.ParseLiteral"/>
    public override object? ParseLiteral(IValueNode valueSyntax)
    {
        if (valueSyntax is StringValueNode s)
        {
            if (decimal.TryParse(s.Value, out decimal dec))
            {
                return Rate.FromRate(dec);
            }
        }

        if (valueSyntax is IntValueNode i)
        {
            if (decimal.TryParse(i.Value, out decimal dec))
            {
                return Rate.FromRate(dec);
            }
        }

        if (valueSyntax is FloatValueNode f)
        {
            if (decimal.TryParse(f.Value, out decimal dec))
            {
                return Rate.FromRate(dec);
            }
        }

        if (valueSyntax is NullValueNode)
        {
            return null;
        }

        throw new SerializationException(
            "Unable to convert the provided value into a Rate value. "
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

        if (runtimeValue is Rate r)
        {
            return new FloatValueNode(decimal.ToSingle(r.Value));
        }

        throw new SerializationException(
            "Unable to convert the provided value into a Rate or null.",
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

        if (runtimeValue is Rate r)
        {
            resultValue = r.Value;

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
                runtimeValue = Rate.FromRate(dec);

                return true;
            }

            return false;
        }

        if (resultValue is int i)
        {
            runtimeValue = Rate.FromRate((decimal)i);

            return true;
        }

        if (resultValue is float f)
        {
            runtimeValue = Rate.FromRate((decimal)f);

            return true;
        }

        if (resultValue is long l)
        {
            runtimeValue = Rate.FromRate((decimal)l);

            return true;
        }

        if (resultValue is decimal d)
        {
            runtimeValue = Rate.FromRate(d);

            return true;
        }

        return false;
    }
}
