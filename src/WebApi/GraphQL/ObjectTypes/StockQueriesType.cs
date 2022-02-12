using HotChocolate.Types;
using JahnDigital.StudentBank.WebApi.GraphQL.Queries;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    public class StockQueriesType : ObjectTypeExtension<StockQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<StockQueries> descriptor)
        {
            descriptor.Name(nameof(Query));
        }
    }
}
