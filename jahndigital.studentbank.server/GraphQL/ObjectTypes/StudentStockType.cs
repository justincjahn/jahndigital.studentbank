using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class StudentStockType : ObjectType<dal.Entities.StudentStock>
    {
        protected override void Configure(IObjectTypeDescriptor<StudentStock> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageStocks}>");
        }
    }
}