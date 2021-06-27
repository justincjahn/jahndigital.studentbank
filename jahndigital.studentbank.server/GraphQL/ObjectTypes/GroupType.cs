using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class GroupType : ObjectType<Group>
    {
        protected override void Configure(IObjectTypeDescriptor<Group> descriptor)
        {
            // Require the user to be a data owner or admin
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageGroups}>");

            // Only administrators can pull in students this way
            descriptor.Field(f => f.Students)
                .Authorize(Constants.Privilege.ManageGroups.Name);
        }
    }
}