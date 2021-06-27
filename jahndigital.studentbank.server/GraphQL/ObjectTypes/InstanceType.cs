using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class InstanceType : ObjectType<Instance>
    {
        protected override void Configure(IObjectTypeDescriptor<Instance> descriptor)
        {
            // Require the user to be a data owner or admin
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageInstances}>");

            // Only administrators can pull in groups this way
            descriptor.Field(f => f.Groups)
                .Authorize(Constants.Privilege.ManageGroups.Name);

            descriptor.Field(f => f.Description).Type<NonNullType<StringType>>();
        }
    }
}