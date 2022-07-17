using System.Runtime.CompilerServices;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Types;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Filtering;

public static class CustomFilterConventionExtensions
{
    public static IFilterConventionDescriptor AddStartsWithInvariant(
        this IFilterConventionDescriptor conventionDescriptor)
    {
        conventionDescriptor
            .Operation(CustomFilterOperations.StartsWithInvariant)
            .Name("startsWithInvariant");

        conventionDescriptor.Configure<StringOperationFilterInputType>(x =>
            x.Operation(CustomFilterOperations.StartsWithInvariant).Type<StringType>()
        );

        return conventionDescriptor;
    }

    public static IFilterProviderDescriptor<QueryableFilterContext> AddStartsWithInvariant(
        this IFilterProviderDescriptor<QueryableFilterContext> providerDescriptor)
    {
        providerDescriptor.AddFieldHandler<StartsWithInvariantFilterHandler>();
        return providerDescriptor;
    }
}
