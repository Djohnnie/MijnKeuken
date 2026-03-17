using MijnKeuken.Application.Common;
using MijnKeuken.Application.Recipes.DTOs;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Web.Services;

public interface IRecipeService
{
    Task<List<RecipeDto>> GetAllAsync();
    Task<RecipeDto?> GetByIdAsync(Guid id);
    Task<Result<Guid>> CreateAsync(CreateRecipeRequest request);
    Task<Result> UpdateAsync(Guid id, CreateRecipeRequest request);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> ArchiveAsync(Guid id);
    Task<Result<ScrapedRecipeDto>> ScrapeFromUrlAsync(string url);
    Task<Result<ScrapedRecipeDto>> ScrapeFromImageAsync(byte[] imageData, string contentType);
}

public record RecipeIngredientRequest(Guid? IngredientId, string FreeText, decimal Amount, UnitType Unit, string CustomUnitDescription, int SortOrder);

public record CreateRecipeRequest(
    string Title, string Description, string Plan, int Servings, string SourceUrl,
    List<Guid> TagIds, List<RecipeIngredientRequest> Ingredients);
