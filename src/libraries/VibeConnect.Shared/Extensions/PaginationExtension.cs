using VibeConnect.Shared.Models;

namespace VibeConnect.Shared.Extensions;

public static class PaginationExtension
{
    public static ApiPagedResult<T> GetPaged<T>(this IEnumerable<T> query, int page, int pageSize) where T : class
    {
        var enumerable = query.ToList();
        var result = new ApiPagedResult<T>
        {
            CurrentPage = page,
            PageSize = pageSize,
            RowCount = enumerable.Count
        };

        var pageCount = (double)result.RowCount / pageSize;
        result.PageCount = (int)Math.Ceiling(pageCount);

        var skip = (page - 1) * pageSize;
        result.Results = enumerable.Skip(skip).Take(pageSize).ToList();

        return result;
    }
}