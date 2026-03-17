using Microsoft.EntityFrameworkCore;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class RecipeRepository(AppDbContext db) : IRecipeRepository
{
    public async Task<Recipe?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Recipes
            .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag)
            .Include(r => r.RecipeIngredients).ThenInclude(ri => ri.Ingredient!)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<Recipe>> GetAllAsync(CancellationToken ct = default)
        => await db.Recipes
            .Where(r => !r.IsArchived)
            .Include(r => r.RecipeTags).ThenInclude(rt => rt.Tag)
            .Include(r => r.RecipeIngredients).ThenInclude(ri => ri.Ingredient!)
            .OrderBy(r => r.Title)
            .ToListAsync(ct);

    public async Task<List<Recipe>> GetArchivedAsync(CancellationToken ct = default)
        => await db.Recipes
            .Where(r => r.IsArchived)
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

    public async Task ReplaceIngredientsAsync(Guid recipeId, List<RecipeIngredient> newIngredients, CancellationToken ct = default)
    {
        // Detach any tracked RecipeIngredients for this recipe to prevent orphan-detection conflicts
        foreach (var entry in db.ChangeTracker.Entries<RecipeIngredient>()
                     .Where(e => e.Entity.RecipeId == recipeId).ToList())
        {
            entry.State = EntityState.Detached;
        }

        // Delete existing ingredients directly from the database
        await db.RecipeIngredients
            .Where(ri => ri.RecipeId == recipeId)
            .ExecuteDeleteAsync(ct);

        // Add new ingredients
        if (newIngredients.Count > 0)
            db.RecipeIngredients.AddRange(newIngredients);
    }

    public async Task<bool> IsInUseAsync(Guid id, CancellationToken ct = default)
        => await db.MenuEntries.AnyAsync(e => e.RecipeId == id, ct);

    public async Task DeleteAsync(Recipe recipe, CancellationToken ct = default)
    {
        db.Recipes.Remove(recipe);
        await db.SaveChangesAsync(ct);
    }
}
