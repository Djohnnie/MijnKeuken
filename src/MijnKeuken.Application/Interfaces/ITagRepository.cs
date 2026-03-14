using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Interfaces;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAndTypeAsync(string name, TagType type, CancellationToken ct = default);
    Task AddAsync(Tag tag, CancellationToken ct = default);
    Task UpdateAsync(Tag tag, CancellationToken ct = default);
    Task DeleteAsync(Tag tag, CancellationToken ct = default);
}
