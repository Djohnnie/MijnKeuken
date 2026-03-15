using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Interfaces;

public interface IStorageLocationRepository
{
    Task<StorageLocation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<StorageLocation>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(StorageLocation location, CancellationToken ct = default);
    Task UpdateAsync(StorageLocation location, CancellationToken ct = default);
    Task DeleteAsync(StorageLocation location, CancellationToken ct = default);
}
