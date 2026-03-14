using MediatR;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Interfaces;

namespace MijnKeuken.Application.Users.Commands.LoginUser;

public class LoginUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username, ct);
        if (user is null)
            return Result<string>.Failure("Ongeldige gebruikersnaam of wachtwoord.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<string>.Failure("Ongeldige gebruikersnaam of wachtwoord.");

        if (!user.IsApproved)
            return Result<string>.Failure("Uw account wacht op goedkeuring.");

        var token = jwtTokenGenerator.GenerateToken(user);
        return Result<string>.Success(token);
    }
}
