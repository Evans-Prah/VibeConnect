using Microsoft.EntityFrameworkCore;
using VibeConnect.Shared.Models;

namespace VibeConnect.Shared.Extensions;

public static class PaginationExtension
{
    public static async Task<ApiPagedResult<T>> GetPaged<T>(this IQueryable<T> query, int page, int pageSize, CancellationToken ct = default) where T : class
    {
        var result = new ApiPagedResult<T>
        {
            PageIndex = page,
            PageSize = pageSize,
            TotalCount = await query.CountAsync(ct),
            LowerBound= page
        };

        var pageCount = (double)result.TotalCount / pageSize;
        result.TotalPages = (int)Math.Ceiling(pageCount);
        result.UpperBound = result.TotalPages;
        result.Results = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return result;
    }
}