using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;
using HotChocolate.Types;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Filtering;

public class StartsWithInvariantFilterHandler : QueryableStringOperationHandler
{
    protected override int Operation => CustomFilterOperations.StartsWithInvariant;

    private static readonly MethodInfo ToLower = typeof(string)
        .GetMethods()
        .Single(
            x => x.Name == nameof(string.ToLower) &&
                x.GetParameters().Length == 0);

    private static readonly MethodInfo StartsWith = typeof(string)
        .GetMethods()
        .First(x => x.Name == nameof(string.StartsWith));

    public StartsWithInvariantFilterHandler(InputParser inputParser) : base(inputParser) { }

    public override Expression HandleOperation(
        QueryableFilterContext context,
        IFilterOperationField field,
        IValueNode value,
        object? parsedValue
    )
    {
        // This is the expression path to the property. e.g. ~> y.FirstName
        Expression property = context.GetInstance();

        // the parsed value is what was specified in the query e.g. ~> eq: "Johnny"
        if (parsedValue is string str)
        {
            // e.g. ~> y.FirstName.ToLower().StartsWith("johnny")
            return Expression.Call(
                Expression.Call(property, ToLower),
                StartsWith,
                Expression.Constant(str.ToLower())
            );
        }

        throw new InvalidOperationException();
    }
}
