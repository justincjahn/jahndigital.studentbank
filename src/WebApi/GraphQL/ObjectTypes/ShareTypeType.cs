using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Enums;
using ShareTypeEntity = JahnDigital.StudentBank.Domain.Entities.ShareType;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    public class ShareTypeType : ObjectType<ShareTypeEntity>
    {
        protected override void Configure(IObjectTypeDescriptor<ShareTypeEntity> descriptor)
        {
            // Require users to be authenticated to access this resource
            descriptor.Authorize();

            // Require admin rights to cascade to shares this way
            descriptor.Field(f => f.Shares)
                .Authorize(Privilege.ManageShares.Name);
        }
    }
}
