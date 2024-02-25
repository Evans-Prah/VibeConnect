using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace VibeConnect.Storage.Services;

public class BaseRepository<T>(ApplicationDbContext dbContext) : IBaseRepository<T>
    where T : class
{
    private readonly DbSet<T> _dbSet = dbContext.Set<T>();

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate)
    {
        var response = await _dbSet.Where(predicate).FirstOrDefaultAsync();
        return response;
    }

    public async Task<int> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return await dbContext.SaveChangesAsync();
    }

    public async Task<int> AddRangeAsync(List<T> entities)
    {
        _dbSet.AddRange(entities);
        return await dbContext.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return await dbContext.SaveChangesAsync();
    }

    public async Task<int> UpdateRangeAsync(List<T> entities)
    {
        _dbSet.UpdateRange(entities);
        return await dbContext.SaveChangesAsync();
    }

    public async Task<int> DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return await dbContext.SaveChangesAsync();
    }

    public IQueryable<T> GetQueryable() => _dbSet.AsNoTracking().AsQueryable();
    public IQueryable<T> GetFromSqlRaw(string sql, object[] parameters) => _dbSet.FromSqlRaw(sql, parameters);
}