using Microsoft.EntityFrameworkCore;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class StorageLocationRepository(AppDbContext db) : IStorageLocationRepository
{
    public async Task<StorageLocation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.StorageLocations.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<List<StorageLocation>> GetAllAsync(CancellationToken ct = default)
        => await db.StorageLocations.OrderBy(l => l.Name).ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await db.StorageLocations.AnyAsync(l => l.Name == name, ct);

    public async Task AddAsync(StorageLocation location, CancellationToken ct = default)
    {
        db.StorageLocations.Add(location);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(StorageLocation location, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(StorageLocation location, CancellationToken ct = default)
    {
        db.StorageLocations.Remove(location);
        await db.SaveChangesAsync(ct);
    }
}
