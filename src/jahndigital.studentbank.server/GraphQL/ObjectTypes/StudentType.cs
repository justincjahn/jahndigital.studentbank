using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class StudentType : ObjectType<Student>
    {
        protected override void Configure(IObjectTypeDescriptor<Student> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStudents}>");

            descriptor.Field(f => f.Group)
                .Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageGroups}>");

            // Hide sensitive fields
            descriptor.Field(f => f.Password).Ignore();
        }
    }
}
