using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Users.Commands.RegisterUser;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Users;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _handler = new RegisterUserHandler(_userRepo.Object, _passwordHasher.Object);
    }

    // The very first user to register should be automatically approved (no existing users to approve them)
    [Fact]
    public async Task FirstUser_ShouldBeAutoApproved()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");

        var result = await _handler.Handle(
            new RegisterUserCommand("admin", "pass123", "admin@test.nl"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _userRepo.Verify(r => r.AddAsync(
            It.Is<User>(u => u.IsApproved && u.Username == "admin"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Any user registering after the first should remain unapproved until an approved user approves them
    [Fact]
    public async Task SubsequentUser_ShouldNotBeAutoApproved()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");

        var result = await _handler.Handle(
            new RegisterUserCommand("newuser", "pass123", "new@test.nl"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _userRepo.Verify(r => r.AddAsync(
            It.Is<User>(u => !u.IsApproved && u.Username == "newuser"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Registration must be rejected when the username is already taken
    [Fact]
    public async Task DuplicateUsername_ShouldFail()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Username = "existing" });

        var result = await _handler.Handle(
            new RegisterUserCommand("existing", "pass123", "dup@test.nl"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // The password must be hashed before being stored — never persisted in plain text
    [Fact]
    public async Task Register_ShouldHashPassword()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AnyUsersExistAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash("mypassword")).Returns("hashed_value");

        await _handler.Handle(
            new RegisterUserCommand("user1", "mypassword", "u@test.nl"), CancellationToken.None);

        _userRepo.Verify(r => r.AddAsync(
            It.Is<User>(u => u.PasswordHash == "hashed_value"),
            It.IsAny<CancellationToken>()), Times.Once);
        _passwordHasher.Verify(h => h.Hash("mypassword"), Times.Once);
    }
}
