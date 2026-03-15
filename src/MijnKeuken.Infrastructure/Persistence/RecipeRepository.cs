using Microsoft.EntityFrameworkCore;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class RecipeRepository(AppDbContext db) : IRecipeRepository
{
    public async Task<Recipe?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Recipes
            .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag)
            .Include(r => r.RecipeIngredients).ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<Recipe>> GetAllAsync(CancellationToken ct = default)
        => await db.Recipes
            .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag)
            .Include(r => r.RecipeIngredients).ThenInclude(ri => ri.Ingredient)
            .OrderBy(r => r.Title)
            .ToListAsync(ct);

    public async Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default)
        => await db.Recipes.AnyAsync(r => r.Title == title, ct);

    public async Task AddAsync(Recipe recipe, CancellationToken ct = default)
    {
        db.Recipes.Add(recipe);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Recipe recipe, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Recipe recipe, CancellationToken ct = default)
    {
        db.Recipes.Remove(recipe);
        await db.SaveChangesAsync(ct);
    }
}
