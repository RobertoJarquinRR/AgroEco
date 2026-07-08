using AgroEco.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgroEco.Data;

public sealed class UnitOfWork<TContext> : IUnitOfWork
    where TContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly TContext _context;

    public UnitOfWork(TContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
