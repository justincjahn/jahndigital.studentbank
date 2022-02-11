using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class StockHistoryType : ObjectType<StockHistory>
    {
        protected override void Configure(IObjectTypeDescriptor<StockHistory> descriptor) { }
    }
}
