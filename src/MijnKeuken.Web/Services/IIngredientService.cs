using MijnKeuken.Application.Common;
using MijnKeuken.Application.Ingredients.DTOs;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Web.Services;

public interface IIngredientService
{
    Task<List<IngredientDto>> GetAllAsync();
    Task<Result<Guid>> CreateAsync(CreateIngredientRequest request);
    Task<Result> UpdateAsync(Guid id, CreateIngredientRequest request);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> ArchiveAsync(Guid id);
    Task<Result<ScrapedIngredientDto>> ScrapeFromUrlAsync(string url);
}

public record CreateIngredientRequest(
    string Title, string Description,
    decimal AmountAvailable, decimal AmountTotal,
    UnitType Unit, string CustomUnitDescription,
    string Barcode, string StoreUrl, bool IsOutOfStock,
    Guid? StorageLocationId, List<Guid> TagIds);
