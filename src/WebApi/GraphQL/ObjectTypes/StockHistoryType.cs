using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    public class StockHistoryType : ObjectType<StockHistory>
    {
        protected override void Configure(IObjectTypeDescriptor<StockHistory> descriptor) { }
    }
}
