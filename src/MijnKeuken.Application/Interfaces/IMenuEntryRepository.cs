using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Interfaces;

public interface IMenuEntryRepository
{
    Task<MenuEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MenuEntry?> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<List<MenuEntry>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task AddAsync(MenuEntry entry, CancellationToken ct = default);
    Task UpdateAsync(MenuEntry entry, CancellationToken ct = default);
    Task DeleteAsync(MenuEntry entry, CancellationToken ct = default);
}
