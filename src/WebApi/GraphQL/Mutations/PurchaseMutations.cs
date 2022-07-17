using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common;
using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Shares.Queries.GetShare;
using JahnDigital.StudentBank.Application.StudentPurchases.Queries.GetStudentPurchase;
using JahnDigital.StudentBank.Application.Transactions.DTOs;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using JahnDigital.StudentBank.Infrastructure.Persistence;
using JahnDigital.StudentBank.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;
using MediatR;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Mutations
{
    /// <summary>
    ///     CRUD operations for <see cref="StudentPurchase" /> entities.
    /// </summary>
    [ExtendObjectType("Mutation")]
    public class PurchaseMutations
    {
        /// <summary>
        ///     Make a new purchase, if authorized.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mediatr"></param>
        /// <param name="resolverContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [UseDbContext(typeof(AppDbContext)), HotChocolate.AspNetCore.Authorization.Authorize]
        public async Task<IQueryable<StudentPurchase>> NewPurchaseAsync(
            PurchaseRequest input,
            [Service] ISender mediatr,
            [SchemaService] IResolverContext resolverContext,
            CancellationToken cancellationToken
        )
        {
            // Fetch the student ID that owns the share and validate they are authorized
            var studentId = await (await mediatr.Send(new GetShareQuery(input.ShareId), cancellationToken))
                .Select(x => (long?)x.StudentId)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Share), input.ShareId);

            await resolverContext
                .SetDataOwner(studentId, UserType.Student)
                .AssertAuthorizedAsync($"{Constants.AuthPolicy.DataOwner}<{Privilege.ManageStudents}>");

            var purchaseId = await mediatr.Send(new PurchaseRequest { ShareId = input.ShareId, Items = input.Items }, cancellationToken);
            return await mediatr.Send(new GetStudentPurchaseQuery(purchaseId), cancellationToken);
        }
    }
}
