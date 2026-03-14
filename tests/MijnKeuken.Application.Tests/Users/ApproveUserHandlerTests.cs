using MijnKeuken.Application.Interfaces;
using MijnKeuken.Application.Users.Commands.ApproveUser;
using MijnKeuken.Domain.Entities;
using Moq;

namespace MijnKeuken.Application.Tests.Users;

public class ApproveUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly ApproveUserHandler _handler;

    public ApproveUserHandlerTests()
    {
        _handler = new ApproveUserHandler(_userRepo.Object);
    }

    // An approved user should be able to approve a pending user successfully
    [Fact]
    public async Task ApprovedUser_CanApproveOthers()
    {
        var approverId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var approver = new User { Id = approverId, Username = "admin", IsApproved = true };
        var target = new User { Id = targetId, Username = "newuser", IsApproved = false };

        _userRepo.Setup(r => r.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approver);
        _userRepo.Setup(r => r.GetByIdAsync(targetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target);

        var result = await _handler.Handle(
            new ApproveUserCommand(approverId, targetId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(target.IsApproved);
        _userRepo.Verify(r => r.UpdateAsync(target, It.IsAny<CancellationToken>()), Times.Once);
    }

    // A user who is not yet approved themselves should not be allowed to approve others
    [Fact]
    public async Task UnapprovedUser_CannotApproveOthers()
    {
        var approverId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var approver = new User { Id = approverId, Username = "pending", IsApproved = false };

        _userRepo.Setup(r => r.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approver);

        var result = await _handler.Handle(
            new ApproveUserCommand(approverId, targetId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // Approval should fail if the approver user ID does not exist in the database
    [Fact]
    public async Task NonExistentApprover_ShouldFail()
    {
        var approverId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        _userRepo.Setup(r => r.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(
            new ApproveUserCommand(approverId, targetId), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    // Approval should fail if the target user ID does not exist in the database
    [Fact]
    public async Task NonExistentTarget_ShouldFail()
    {
        var approverId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var approver = new User { Id = approverId, IsApproved = true };

        _userRepo.Setup(r => r.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approver);
        _userRepo.Setup(r => r.GetByIdAsync(targetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(
            new ApproveUserCommand(approverId, targetId), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    // Approving a user who is already approved should fail to prevent redundant operations
    [Fact]
    public async Task AlreadyApprovedTarget_ShouldFail()
    {
        var approverId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var approver = new User { Id = approverId, IsApproved = true };
        var target = new User { Id = targetId, IsApproved = true };

        _userRepo.Setup(r => r.GetByIdAsync(approverId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approver);
        _userRepo.Setup(r => r.GetByIdAsync(targetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(target);

        var result = await _handler.Handle(
            new ApproveUserCommand(approverId, targetId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
