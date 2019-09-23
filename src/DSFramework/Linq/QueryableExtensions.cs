using System;
using System.Linq;
using System.Linq.Expressions;
using DSFramework.GuardToolkit;

namespace DSFramework.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            Check.ArgumentNotNull(query, nameof(query));

            return condition ? query.Where(predicate) : query;
        }

        public static IQueryable<T> TakeIf<T, TKey>(
            this IQueryable<T> query,
            Expression<Func<T, TKey>> orderBy,
            bool condition,
            int limit,
            bool orderByDescending = true)
        {
            query = orderByDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

            return condition ? query.Take(limit) : query;
        }

        public static IQueryable<T> PageBy<T, TKey>(
            this IQueryable<T> query,
            Expression<Func<T, TKey>> orderBy,
            int page,
            int pageSize,
            bool orderByDescending = true)
        {
            const int defaultPageNumber = 1;

            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (page <= 0)
            {
                page = defaultPageNumber;
            }

            query = orderByDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
        {
            Check.ArgumentNotNull(query, nameof(query));

            var skip = (page - 1) * pageSize;
            var take = pageSize;

            return query.Skip(skip).Take(take);
        }
    }
}