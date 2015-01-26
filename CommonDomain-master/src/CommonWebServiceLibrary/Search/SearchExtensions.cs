using System;
using System.Linq;
using System.Linq.Expressions;
using NLog;

namespace CommonWebServiceLibrary.Search
{
    public static class SearchExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IQueryable<T> OrderByField<T>(this IQueryable<T> q, string sortField, SortDirections direction)
        {
            var param = Expression.Parameter(typeof(T), "p");

            try
            {
                var prop = Expression.Property(param, sortField);
                var exp = Expression.Lambda(prop, param);

                string method = direction == SortDirections.Ascending ? "OrderBy" : "OrderByDescending";
                Type[] types = new Type[] { q.ElementType, exp.Body.Type };

                var mcExp = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
                return q.Provider.CreateQuery<T>(mcExp);
            }
            catch (ArgumentException e)
            {
                Logger.ErrorException(string.Format("'{0}' Property or field not found.", sortField ) , e);
                throw;
            }
            catch (Exception ex)
            {
                Logger.ErrorException("OrderByField has been failed.", ex);
                throw;
            }
           
        }
    }
}
