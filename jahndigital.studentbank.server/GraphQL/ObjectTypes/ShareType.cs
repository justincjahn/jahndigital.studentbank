using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class ShareType : ObjectType<dal.Entities.Share>
    {
        protected override void Configure(IObjectTypeDescriptor<Share> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageShares.Name}>");
            
            // Require transactions to be queried separately to prevent performance issues.
            descriptor.Field(f => f.Transactions).Ignore();

            // Hide raw balance
            descriptor.Field(f => f.RawBalance).Ignore();
        }
    }
}
