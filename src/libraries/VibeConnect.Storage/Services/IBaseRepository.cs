using System.Linq.Expressions;

namespace VibeConnect.Storage.Services;

public interface IBaseRepository<T>
{
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate);
    Task<int> AddAsync(T entity);
    Task<int> AddRangeAsync(List<T> entities);
    Task<int> UpdateAsync(T entity);
    Task<int> UpdateRangeAsync(List<T> entities);
    Task<int> DeleteAsync(T entity);
    IQueryable<T> GetQueryable();
    IQueryable<T> GetFromSqlRaw(string sql, object[] parameters);
}