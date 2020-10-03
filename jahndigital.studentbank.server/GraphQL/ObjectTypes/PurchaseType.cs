using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class PurchaseType : ObjectType<dal.Entities.StudentPurchase>
    {
        protected override void Configure(IObjectTypeDescriptor<StudentPurchase> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManagePurchases}>");
        }
    }
}
