using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    public class GroupType : ObjectType<Group>
    {
        protected override void Configure(IObjectTypeDescriptor<Group> descriptor)
        {
            // descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageGroups}>");
            descriptor.Authorize();

            // Only administrators can pull in students this way
            descriptor.Field(f => f.Students)
                .Authorize(Privilege.ManageGroups.Name);
        }
    }
}
