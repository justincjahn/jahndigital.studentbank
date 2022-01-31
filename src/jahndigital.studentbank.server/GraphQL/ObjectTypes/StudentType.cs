using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class StudentType : ObjectType<Student>
    {
        protected override void Configure(IObjectTypeDescriptor<Student> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStudents}>");

            descriptor.Field(f => f.Group)
                .Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageGroups}>");

            // Hide sensitive fields
            descriptor.Field(f => f.Password).Ignore();
            descriptor.Field(f => f.ValidatePassword(default!)).Ignore();
        }
    }
}
