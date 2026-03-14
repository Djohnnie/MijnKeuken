using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> AnyUsersExistAsync(CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task<List<User>> GetPendingUsersAsync(CancellationToken ct = default);
    Task<List<User>> GetAllUsersAsync(CancellationToken ct = default);
}
