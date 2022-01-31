using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;
using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class TransactionTypes : ObjectType<Transaction>
    {
        protected override void Configure(IObjectTypeDescriptor<Transaction> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Constants.Privilege.ManageTransactions.Name}>");
        }
    }
}
