using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class TransactionTypes : ObjectType<dal.Entities.Transaction>
    {
        protected override void Configure(IObjectTypeDescriptor<Transaction> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageTransactions.Name}>");
        }
    }
}