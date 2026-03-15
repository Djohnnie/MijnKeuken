using Microsoft.EntityFrameworkCore;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class MenuEntryRepository(AppDbContext db) : IMenuEntryRepository
{
    public async Task<MenuEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.MenuEntries
            .Include(e => e.Recipe)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<MenuEntry?> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await db.MenuEntries
            .Include(e => e.Recipe)
            .FirstOrDefaultAsync(e => e.Date == date, ct);

    public async Task<List<MenuEntry>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        => await db.MenuEntries
            .Include(e => e.Recipe)
            .Where(e => e.Date >= from && e.Date <= to)
            .OrderBy(e => e.Date)
            .ToListAsync(ct);

    public async Task AddAsync(MenuEntry entry, CancellationToken ct = default)
    {
        db.MenuEntries.Add(entry);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MenuEntry entry, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(MenuEntry entry, CancellationToken ct = default)
    {
        db.MenuEntries.Remove(entry);
        await db.SaveChangesAsync(ct);
    }
}
