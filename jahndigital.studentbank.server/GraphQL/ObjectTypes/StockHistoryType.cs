using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class StockHistoryType : ObjectType<dal.Entities.StockHistory>
    {
        protected override void Configure(IObjectTypeDescriptor<StockHistory> descriptor) { }
    }
}
