using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Users.Queries.GetUserProfile;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Users;

public class GetUserProfileHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly GetUserProfileHandler _handler;

    public GetUserProfileHandlerTests()
    {
        _handler = new GetUserProfileHandler(_userRepo.Object);
    }

    // Retrieving a profile for a valid user ID should return all profile fields correctly
    [Fact]
    public async Task ExistingUser_ShouldReturnProfile()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@test.nl",
            IsApproved = true,
            PrefersDarkMode = true,
            CreatedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(
            new GetUserProfileQuery(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("testuser", result.Value!.Username);
        Assert.Equal("test@test.nl", result.Value.Email);
        Assert.True(result.Value.IsApproved);
        Assert.True(result.Value.PrefersDarkMode);
    }

    // Requesting a profile for an unknown user ID should return a failure result
    [Fact]
    public async Task NonExistentUser_ShouldFail()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(
            new GetUserProfileQuery(userId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }
}
