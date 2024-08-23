using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    public class InstanceType : ObjectType<Instance>
    {
        protected override void Configure(IObjectTypeDescriptor<Instance> descriptor)
        {
            // descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageInstances}>");
            descriptor.Authorize();

            // Only administrators can pull in groups this way
            descriptor.Field(f => f.Groups)
                .Authorize(Privilege.ManageGroups.Name);

            descriptor.Field(f => f.Description).Type<NonNullType<StringType>>();
        }
    }
}
