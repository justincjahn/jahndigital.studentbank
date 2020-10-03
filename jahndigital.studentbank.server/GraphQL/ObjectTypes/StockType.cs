using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class StockType : ObjectType<dal.Entities.Stock>
    {
        protected override void Configure(IObjectTypeDescriptor<Stock> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStocks}>");
        }
    }
}
