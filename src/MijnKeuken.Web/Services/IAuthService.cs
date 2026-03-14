using MijnKeuken.Application.Common;

namespace MijnKeuken.Web.Services;

public interface IAuthService
{
    Task<Result> RegisterAsync(string username, string password, string email);
    Task<Result<string>> LoginAsync(string username, string password);
}
