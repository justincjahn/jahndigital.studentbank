using HotChocolate.Types;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class StudentType : ObjectType<dal.Entities.Student>
    {
        protected override void Configure(IObjectTypeDescriptor<dal.Entities.Student> descriptor)
        {
            descriptor.Field(f => f.Group)
                .Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStudents.Name}>");
            
            // Hide sensitive fields
            descriptor.Field(f => f.Password).Ignore(true);
            descriptor.Field(f => f.ValidatePassword(default)).Ignore(true);
        }
    }
}