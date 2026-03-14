using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Users.Commands.UpdateThemePreference;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Users;

public class UpdateThemePreferenceHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly UpdateThemePreferenceHandler _handler;

    public UpdateThemePreferenceHandlerTests()
    {
        _handler = new UpdateThemePreferenceHandler(_userRepo.Object);
    }

    // Updating theme preference for an existing user should persist the new value
    [Fact]
    public async Task ExistingUser_ShouldUpdatePreference()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.nl",
            IsApproved = true,
            PrefersDarkMode = false
        };
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(
            new UpdateThemePreferenceCommand(userId, true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(user.PrefersDarkMode);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Updating theme preference for a non-existent user should return a failure
    [Fact]
    public async Task NonExistentUser_ShouldFail()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(
            new UpdateThemePreferenceCommand(userId, true), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Toggling dark mode off should set PrefersDarkMode to false
    [Fact]
    public async Task TogglingOff_ShouldSetFalse()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.nl",
            IsApproved = true,
            PrefersDarkMode = true
        };
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(
            new UpdateThemePreferenceCommand(userId, false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(user.PrefersDarkMode);
    }
}
