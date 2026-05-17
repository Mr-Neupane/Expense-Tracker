using ExpenseTracker.Models;

namespace ExpenseTracker.UnitOfWork.Interfaces;

public interface IUow : IDisposable
{
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();

    Task AddAsync<T>(T entity) where T : class, IEntity;
    void Update<T>(T entity) where T : class, IEntity;
    void Remove<T>(T entity) where T : class, IEntity;
    Task SoftDeleteAsync<T>(int id) where T : class, IEntity;
    Task SoftDeleteAsync<T>(IEnumerable<int> ids) where T : class, IEntity;
}
