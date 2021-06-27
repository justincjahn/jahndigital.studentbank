using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class PurchaseType : ObjectType<StudentPurchase>
    {
        protected override void Configure(IObjectTypeDescriptor<StudentPurchase> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManagePurchases}>");
        }
    }
}