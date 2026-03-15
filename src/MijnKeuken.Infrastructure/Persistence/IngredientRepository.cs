using Microsoft.EntityFrameworkCore;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class IngredientRepository(AppDbContext db) : IIngredientRepository
{
    public async Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Ingredients
            .Include(i => i.StorageLocation)
            .Include(i => i.IngredientTags)
                .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<List<Ingredient>> GetAllAsync(CancellationToken ct = default)
        => await db.Ingredients
            .Include(i => i.StorageLocation)
            .Include(i => i.IngredientTags)
                .ThenInclude(it => it.Tag)
            .OrderBy(i => i.Title)
            .ToListAsync(ct);

    public async Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default)
        => await db.Ingredients.AnyAsync(i => i.Title == title, ct);

    public async Task AddAsync(Ingredient ingredient, CancellationToken ct = default)
    {
        db.Ingredients.Add(ingredient);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Ingredient ingredient, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Ingredient ingredient, CancellationToken ct = default)
    {
        db.Ingredients.Remove(ingredient);
        await db.SaveChangesAsync(ct);
    }
}
