using MijnKeuken.Application.Common;
using MijnKeuken.Application.StorageLocations.DTOs;

namespace MijnKeuken.Web.Services;

public interface IStorageLocationService
{
    Task<List<StorageLocationDto>> GetAllAsync();
    Task<Result<Guid>> CreateAsync(string name, string description);
    Task<Result> UpdateAsync(Guid id, string name, string description);
    Task<Result> DeleteAsync(Guid id);
}
