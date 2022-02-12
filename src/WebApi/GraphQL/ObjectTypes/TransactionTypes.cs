using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    public class TransactionTypes : ObjectType<Transaction>
    {
        protected override void Configure(IObjectTypeDescriptor<Transaction> descriptor)
        {
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageTransactions.Name}>");
        }
    }
}
