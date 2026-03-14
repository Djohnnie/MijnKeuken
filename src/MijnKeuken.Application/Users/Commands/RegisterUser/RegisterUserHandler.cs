using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Application.Users.Commands.RegisterUser;

public class RegisterUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterUserCommand, Result>
{
    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var existing = await userRepository.GetByUsernameAsync(request.Username, ct);
        if (existing is not null)
            return Result.Failure("Gebruikersnaam is al in gebruik.");

        var isFirstUser = !await userRepository.AnyUsersExistAsync(ct);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = passwordHasher.Hash(request.Password),
            Email = request.Email,
            IsApproved = isFirstUser,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, ct);

        return Result.Success();
    }
}
