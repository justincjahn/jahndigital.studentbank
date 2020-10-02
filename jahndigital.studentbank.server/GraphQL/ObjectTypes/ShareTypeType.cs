using HotChocolate.Types;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class ShareTypeType : ObjectType<dal.Entities.ShareType>
    {
        protected override void Configure(IObjectTypeDescriptor<dal.Entities.ShareType> descriptor)
        {
            descriptor.Authorize();

            // Hide the raw rate
            descriptor.Field(f => f.RawDividendRate).Ignore();
        }
    }
}
