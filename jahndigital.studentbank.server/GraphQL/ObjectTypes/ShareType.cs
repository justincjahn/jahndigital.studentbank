using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class ShareType : ObjectType<dal.Entities.Share>
    {
        protected override void Configure(IObjectTypeDescriptor<Share> descriptor)
        {
            // Require the user to be a data owner or admin
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageShares}>");
            
            // Require transactions to be queried separately to prevent performance issues.
            descriptor.Field(f => f.Transactions).Ignore();
        }
    }
}
