using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class InstanceType : ObjectType<Instance>
    {
        protected override void Configure(IObjectTypeDescriptor<Instance> descriptor)
        {
            // Require the user to be a data owner or admin
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageInstances}>");

            // Only administrators can pull in groups this way
            descriptor.Field(f => f.Groups)
                .Authorize(Privilege.ManageGroups.Name);

            descriptor.Field(f => f.Description).Type<NonNullType<StringType>>();
        }
    }
}
