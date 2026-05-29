using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using ExpenseTracker.Enums;

namespace ExpenseTracker.UnitOfWork;

public class Uow : IUow
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public Uow(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
    }

    public async Task AddAsync<T>(T entity) where T : class, IEntity
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public void Update<T>(T entity) where T : class, IEntity
    {
        _context.Set<T>().Update(entity);
    }

    public void Remove<T>(T entity) where T : class, IEntity
    {
        _context.Set<T>().Remove(entity);
    }

    public async Task SoftDeleteAsync<T>(int id) where T : class, IEntity
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"{typeof(T).Name} with id {id} not found");
        if (entity is BaseModel baseEntity)
        {
            baseEntity.Status = Status.Reversed;
             _context.Update(baseEntity);
        }
    }

    public async Task SoftDeleteAsync<T>(IEnumerable<int> ids) where T : class, IEntity
    {
        foreach (var id in ids)
        {
            await SoftDeleteAsync<T>(id);
        }
    }
}
