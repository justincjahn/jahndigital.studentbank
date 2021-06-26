using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class UserTypes : ObjectType<dal.Entities.User>
    {
        protected override void Configure(IObjectTypeDescriptor<User> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageUsers.Name}>");

            // Hide sensitive fields
            descriptor.Field(f => f.Password).Ignore();
            descriptor.Field(f => f.ValidatePassword(default!)).Ignore();
            descriptor.Field(f => f.RefreshTokens).Ignore();
        }
    }
}
