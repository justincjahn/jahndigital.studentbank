using HotChocolate.Data;
using HotChocolate.Data.Filters;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Filtering;

public class CustomFilterConvention : FilterConvention
{
    protected override void Configure(IFilterConventionDescriptor descriptor)
    {
        descriptor.AddDefaults();
        descriptor.AddStartsWithInvariant();
        descriptor.Provider(new CustomQueryableFilterProvider());
    }
}
