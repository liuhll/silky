using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Silky.Core.Extensions.Collections.Generic;

namespace System.Linq;

public static class QueryableExtensions
{
    private class SortByInfo
    {
        public SortDirection Direction { get; set; }
        public string PropertyName { get; set; }
        public bool Initial { get; set; }
    }

    private enum SortDirection
    {
        Ascending = 0,
        Descending = 1
    }

    public static IEnumerable<T> SortByClause<T>
        (this IEnumerable<T> enumerable, string sortBy)
    {
        return enumerable.AsQueryable().SortBy(sortBy).AsEnumerable();
    }

    public static IQueryable<T> SortBy<T>
        (this IQueryable<T> queryable, string sortBy)
    {
        foreach (SortByInfo sortByInfo in ParseOrderBy(sortBy))
            queryable = ApplyOrderBy<T>(queryable, sortByInfo);

        return queryable;
    }

    private static IEnumerable<SortByInfo> ParseOrderBy(string sortBy)
    {
        if (sortBy.IsNullOrEmpty())
            yield break;

        string[] items = sortBy.Split(',');
        bool initial = true;
        foreach (string item in items)
        {
            string[] pair = item.Trim().Split('-');

            if (pair.Length > 2)
                throw new ArgumentException(
                    $"Invalid OrderBy string '{item}'. Order By Format: Property,Property2-DESC, Property2-ASC");

            string prop = pair[0].Trim();

            if (String.IsNullOrEmpty(prop))
                throw new ArgumentException(
                    "Invalid Property. Order By Format: Property, Property2-DESC, Property2-ASC");

            SortDirection dir = SortDirection.Ascending;

            if (pair.Length == 2)
                dir = ("desc".Equals(pair[1].Trim(),
                    StringComparison.OrdinalIgnoreCase)
                    ? SortDirection.Descending
                    : SortDirection.Ascending);

            yield return new SortByInfo()
            {
                PropertyName = prop,
                Direction = dir, Initial = initial
            };

            initial = false;
        }
    }

    private static IQueryable<T> ApplyOrderBy<T>
        (IQueryable<T> collection, SortByInfo sortByInfo)
    {
        string[] props = sortByInfo.PropertyName.Split('.');
        Type type = typeof(T);

        var arg = Expression.Parameter(type, "x");
        Expression expr = arg;
        foreach (string prop in props)
        {
            PropertyInfo pi = type.GetProperty(prop);
            expr = Expression.Property(expr, pi);
            type = pi.PropertyType;
        }

        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
        LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);
        string methodName = String.Empty;

        if (!sortByInfo.Initial && collection is IOrderedQueryable<T>)
        {
            if (sortByInfo.Direction == SortDirection.Ascending)
                methodName = "ThenBy";
            else
                methodName = "ThenByDescending";
        }
        else
        {
            if (sortByInfo.Direction == SortDirection.Ascending)
                methodName = "OrderBy";
            else
                methodName = "OrderByDescending";
        }

        return (IOrderedQueryable<T>)typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                          && method.IsGenericMethodDefinition
                          && method.GetGenericArguments().Length == 2
                          && method.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), type)
            .Invoke(null, new object[] { collection, lambda });
    }
}