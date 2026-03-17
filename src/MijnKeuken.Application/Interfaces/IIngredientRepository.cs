using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Interfaces;

public interface IIngredientRepository
{
    Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Ingredient>> GetAllAsync(CancellationToken ct = default);
    Task<List<Ingredient>> GetArchivedAsync(CancellationToken ct = default);
    Task<bool> ExistsByTitleAsync(string title, CancellationToken ct = default);
    Task AddAsync(Ingredient ingredient, CancellationToken ct = default);
    Task UpdateAsync(Ingredient ingredient, CancellationToken ct = default);
    Task<bool> IsInUseAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Ingredient ingredient, CancellationToken ct = default);
}
