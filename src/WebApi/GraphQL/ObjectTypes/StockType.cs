using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    public class StockType : ObjectType<Stock>
    {
        protected override void Configure(IObjectTypeDescriptor<Stock> descriptor)
        {
            // descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStocks}>");
            descriptor.Authorize();
        }
    }
}
