using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    public class ShareType : ObjectType<Share>
    {
        protected override void Configure(IObjectTypeDescriptor<Share> descriptor)
        {
            // Require the user to be a data owner or admin
            descriptor.Authorize($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageShares}>");

            // Require transactions to be queried separately to prevent performance issues.
            descriptor.Field(f => f.Transactions).Ignore();
        }
    }
}
