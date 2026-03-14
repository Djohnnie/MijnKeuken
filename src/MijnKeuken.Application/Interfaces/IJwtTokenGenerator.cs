using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
