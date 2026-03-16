using Microsoft.EntityFrameworkCore;
using MijnKeuken.Application.Dashboard.DTOs;
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

    public async Task<List<RecipeUsageStatDto>> GetTopScheduledRecipesAsync(int count, CancellationToken ct = default)
    {
        var results = await db.MenuEntries
            .Where(e => e.RecipeId != null)
            .Join(db.Set<Recipe>(), e => e.RecipeId, r => r.Id, (e, r) => new { e.RecipeId, r.Title })
            .GroupBy(x => new { x.RecipeId, x.Title })
            .Select(g => new { g.Key.RecipeId, g.Key.Title, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(count)
            .ToListAsync(ct);

        return results.Select(r => new RecipeUsageStatDto(r.RecipeId!.Value, r.Title, r.Count)).ToList();
    }

    public async Task<List<IngredientUsageStatDto>> GetTopUsedIngredientsAsync(int count, CancellationToken ct = default)
    {
        var results = await db.MenuEntries
            .Where(e => e.RecipeId != null)
            .Join(db.Set<RecipeIngredient>(), e => e.RecipeId, ri => ri.RecipeId, (e, ri) => ri)
            .Where(ri => ri.IngredientId != null)
            .Join(db.Set<Ingredient>(), ri => ri.IngredientId!.Value, i => i.Id, (ri, i) => new { IngredientId = ri.IngredientId!.Value, i.Title })
            .GroupBy(x => new { x.IngredientId, x.Title })
            .Select(g => new { g.Key.IngredientId, g.Key.Title, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(count)
            .ToListAsync(ct);

        return results.Select(r => new IngredientUsageStatDto(r.IngredientId, r.Title, r.Count)).ToList();
    }

    public async Task<NextDeliveryDto?> GetNextDeliveryAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var entry = await db.MenuEntries
            .Where(e => e.HasDelivery && e.Date >= today)
            .OrderBy(e => e.Date)
            .Select(e => new { e.Date, e.DeliveryNote })
            .FirstOrDefaultAsync(ct);

        return entry is not null ? new NextDeliveryDto(entry.Date, entry.DeliveryNote) : null;
    }
}
