using HotChocolate.Data;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Filtering;

public class CustomQueryableFilterProvider : QueryableFilterProvider
{
    protected override void Configure(IFilterProviderDescriptor<QueryableFilterContext> descriptor)
    {
        descriptor.AddDefaultFieldHandlers();
        descriptor.AddStartsWithInvariant();
    }
}
