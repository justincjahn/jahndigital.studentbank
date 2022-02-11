using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class GroupType : ObjectType<Group>
    {
        protected override void Configure(IObjectTypeDescriptor<Group> descriptor)
        {
            // Require the user to be a data owner or admin
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageGroups}>");

            // Only administrators can pull in students this way
            descriptor.Field(f => f.Students)
                .Authorize(Privilege.ManageGroups.Name);
        }
    }
}
