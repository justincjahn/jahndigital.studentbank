using HotChocolate.Resolvers;
using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class ShareType : ObjectType<dal.Entities.Share>
    {
        protected override void Configure(IObjectTypeDescriptor<Share> descriptor)
        {
            
        }
    }
}