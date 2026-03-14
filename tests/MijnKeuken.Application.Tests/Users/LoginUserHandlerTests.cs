using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Users.Commands.LoginUser;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Users;

public class LoginUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwtGenerator = new();
    private readonly LoginUserHandler _handler;

    public LoginUserHandlerTests()
    {
        _handler = new LoginUserHandler(
            _userRepo.Object, _passwordHasher.Object, _jwtGenerator.Object);
    }

    // An approved user with correct credentials should receive a JWT token
    [Fact]
    public async Task ValidCredentials_ApprovedUser_ShouldReturnToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = "hashed",
            Email = "admin@test.nl",
            IsApproved = true
        };
        _userRepo.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("pass123", "hashed")).Returns(true);
        _jwtGenerator.Setup(g => g.GenerateToken(user)).Returns("jwt-token-123");

        var result = await _handler.Handle(
            new LoginUserCommand("admin", "pass123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token-123", result.Value);
    }

    // Login with a non-existent username should fail without generating a token
    [Fact]
    public async Task UnknownUsername_ShouldFail()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(
            new LoginUserCommand("unknown", "pass123"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _jwtGenerator.Verify(g => g.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    // Login with an incorrect password should fail without generating a token
    [Fact]
    public async Task WrongPassword_ShouldFail()
    {
        var user = new User { Username = "admin", PasswordHash = "hashed", IsApproved = true };
        _userRepo.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

        var result = await _handler.Handle(
            new LoginUserCommand("admin", "wrong"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _jwtGenerator.Verify(g => g.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    // A user who has not yet been approved should be denied login even with correct credentials
    [Fact]
    public async Task UnapprovedUser_ShouldFail()
    {
        var user = new User
        {
            Username = "pending",
            PasswordHash = "hashed",
            IsApproved = false
        };
        _userRepo.Setup(r => r.GetByUsernameAsync("pending", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("pass123", "hashed")).Returns(true);

        var result = await _handler.Handle(
            new LoginUserCommand("pending", "pass123"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("goedkeuring", result.Error!, StringComparison.OrdinalIgnoreCase);
        _jwtGenerator.Verify(g => g.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    // Wrong password and unknown username should return the same error to prevent user enumeration
    [Fact]
    public async Task WrongPassword_ShouldReturnSameErrorAsUnknownUser()
    {
        var user = new User { Username = "admin", PasswordHash = "hashed", IsApproved = true };
        _userRepo.Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

        var wrongPwResult = await _handler.Handle(
            new LoginUserCommand("admin", "wrong"), CancellationToken.None);

        _userRepo.Setup(r => r.GetByUsernameAsync("nobody", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var unknownResult = await _handler.Handle(
            new LoginUserCommand("nobody", "pass"), CancellationToken.None);

        Assert.Equal(wrongPwResult.Error, unknownResult.Error);
    }
}
