using ExpenseTracker.Enums;
using ExpenseTracker.Models;

namespace ExpenseTracker.ExtMethods;

public static class QueryableExtensions
{
    public static IQueryable<T> Active<T>(this IQueryable<T> query)
        where T : BaseModel
    {
        return query.Where(e => e.Status == Status.Active);
    }
}
