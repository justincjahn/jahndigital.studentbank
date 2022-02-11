using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class UserTypes : ObjectType<User>
    {
        protected override void Configure(IObjectTypeDescriptor<User> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageUsers.Name}>");

            // Hide sensitive fields
            descriptor.Field(f => f.Password).Ignore();
            descriptor.Field(f => f.RefreshTokens).Ignore();
        }
    }
}
