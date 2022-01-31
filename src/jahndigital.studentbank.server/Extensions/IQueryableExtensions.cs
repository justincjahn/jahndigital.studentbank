using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace jahndigital.studentbank.server.Extensions
{
#nullable disable

    /// <summary>
    ///     Extensions to the <see cref="IQueryable" /> interface.
    /// </summary>
    public static class IQueryableExtensions
    {
        private static object Private(this object obj, string privateField)
        {
            return obj?.GetType()
                .GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(obj);
        }

        private static T Private<T>(this object obj, string privateField)
        {
            return (T)obj?.GetType()
                .GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(obj);
        }

        /// <summary>
        ///     Convert to a SQL string... Doesn't always work!
        /// </summary>
        public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            IEnumerator<TEntity> enumerator =
                query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            object relationalCommandCache = enumerator.Private("_relationalCommandCache");
            SelectExpression selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
            IQuerySqlGeneratorFactory factory =
                relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

            QuerySqlGenerator sqlGenerator = factory.Create();
            IRelationalCommand command = sqlGenerator.GetCommand(selectExpression);

            string sql = command.CommandText;

            return sql;
        }
    }
}
