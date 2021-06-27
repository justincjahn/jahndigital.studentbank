using HotChocolate.Types;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class ShareTypeType : ObjectType<dal.Entities.ShareType>
    {
        protected override void Configure(IObjectTypeDescriptor<dal.Entities.ShareType> descriptor)
        {
            // Require users to be authenticated to access this resource
            descriptor.Authorize();

            // Require admin rights to cascade to shares this way
            descriptor.Field(f => f.Shares)
                .Authorize(Constants.Privilege.ManageShares.Name);
        }
    }
}