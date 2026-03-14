using Microsoft.EntityFrameworkCore;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task<bool> AnyUsersExistAsync(CancellationToken ct = default)
        => await db.Users.AnyAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<User>> GetPendingUsersAsync(CancellationToken ct = default)
        => await db.Users.Where(u => !u.IsApproved).ToListAsync(ct);

    public async Task<List<User>> GetAllUsersAsync(CancellationToken ct = default)
        => await db.Users.OrderBy(u => u.Username).ToListAsync(ct);
}
