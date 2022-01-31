using HotChocolate.Types;
using jahndigital.studentbank.server.GraphQL.Queries;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class StockQueriesType : ObjectTypeExtension<StockQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<StockQueries> descriptor)
        {
            descriptor.Name(nameof(Query));
        }
    }
}
