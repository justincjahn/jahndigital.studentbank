using System.Linq;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    /// <summary>
    /// Allows students to list their purchases and admins to list all purchases.
    /// </summary>
    [ExtendObjectType(Name = "Query")]
    public class PurchaseQueries
    {
        /// <summary>
        /// Get a list of purchases for a specific student.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSelection, UseSorting]
        public IQueryable<dal.Entities.StudentPurchase> GetStudentPurchases(long studentId, [Service]AppDbContext context) =>
            context.StudentPurchases.Where(x => x.StudentId == studentId);
        
        /// <summary>
        /// Get a list of purchases for all students.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSelection, UseSorting]
        public IQueryable<dal.Entities.StudentPurchase> GetPurchases([Service]AppDbContext context) =>
            context.StudentPurchases;
    }
}
