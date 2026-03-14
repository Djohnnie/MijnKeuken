using Microsoft.EntityFrameworkCore;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class TagRepository(AppDbContext db) : ITagRepository
{
    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<List<Tag>> GetAllAsync(CancellationToken ct = default)
        => await db.Tags.OrderBy(t => t.Type).ThenBy(t => t.Name).ToListAsync(ct);

    public async Task<bool> ExistsByNameAndTypeAsync(string name, TagType type, CancellationToken ct = default)
        => await db.Tags.AnyAsync(t => t.Name == name && t.Type == type, ct);

    public async Task AddAsync(Tag tag, CancellationToken ct = default)
    {
        db.Tags.Add(tag);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tag tag, CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Tag tag, CancellationToken ct = default)
    {
        db.Tags.Remove(tag);
        await db.SaveChangesAsync(ct);
    }
}
