namespace ECommerce.Application.Interfaces;

public interface IRepository<T> where T : class
{
    // Query
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    IQueryable<T> Query();

    // Commands
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);

    // Utilities
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync();
    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
}
