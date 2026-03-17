using MijnKeuken.Application.Archive.DTOs;
using MijnKeuken.Application.Common;

namespace MijnKeuken.Web.Services;

public interface IArchiveService
{
    Task<List<ArchivedItemDto>> GetAllAsync();
    Task<Result> UnarchiveRecipeAsync(Guid id);
    Task<Result> UnarchiveIngredientAsync(Guid id);
}
