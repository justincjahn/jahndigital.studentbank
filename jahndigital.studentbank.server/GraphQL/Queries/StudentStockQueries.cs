using System.Linq;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using jahndigital.studentbank.dal.Contexts;

namespace jahndigital.studentbank.server.GraphQL.Queries
{
    [ExtendObjectType(Name = "Query")]
    public class StudentStockQueries
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSelection, UseSorting]
        public IQueryable<dal.Entities.StudentStock> GetStudentStock(long studentId, [Service]AppDbContext context) =>
            context.StudentStocks.Where(x => x.StudentId == studentId);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [UsePaging, UseFiltering, UseSelection, UseSorting]
        public IQueryable<dal.Entities.StudentStock> GetStudentStocks([Service]AppDbContext context) =>
            context.StudentStocks;
    }
}
