using ECommerce.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IQueryable to support pagination
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Convert IQueryable to PagedResponse with automatic counting and paging
    /// </summary>
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<T>(items, request.Page, request.PageSize, totalCount);
    }

    /// <summary>
    /// Apply sorting to IQueryable based on property name
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sortBy,
        bool descending = false)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        try
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
            var property = System.Linq.Expressions.Expression.Property(parameter, sortBy);
            var lambda = System.Linq.Expressions.Expression.Lambda(property, parameter);

            var methodName = descending ? "OrderByDescending" : "OrderBy";
            var resultExpression = System.Linq.Expressions.Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.Type },
                query.Expression,
                System.Linq.Expressions.Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
        catch
        {
            // If sorting fails, return original query
            return query;
        }
    }
}
