using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Interfaces;

public interface IRecipeRepository
{
    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Recipe>> GetAllAsync(CancellationToken ct = default);
    Task<List<Recipe>> GetArchivedAsync(CancellationToken ct = default);
    Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default);
    Task AddAsync(Recipe recipe, CancellationToken ct = default);
    Task UpdateAsync(Recipe recipe, CancellationToken ct = default);
    Task ReplaceIngredientsAsync(Guid recipeId, List<RecipeIngredient> newIngredients, CancellationToken ct = default);
    Task<bool> IsInUseAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Recipe recipe, CancellationToken ct = default);
}
