using System.Linq.Expressions;
using ExpenseTracker.Models;

namespace ExpenseTracker.Repository;

public interface IGenericRepository<T> where T : class, IEntity
{
    IQueryable<T> GetBaseQueryable();
    Task<T> FindOrThrowAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAsync(Expression<Func<T, bool>> predicate);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}
