using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace MijnKeuken.Web.Auth;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public string? CurrentToken { get; private set; }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_currentUser));

    public void SetToken(string token)
    {
        CurrentToken = token;

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        _currentUser = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void ClearToken()
    {
        CurrentToken = null;
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public Guid? GetCurrentUserId()
    {
        var sub = _currentUser.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? _currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return sub is not null && Guid.TryParse(sub, out var id) ? id : null;
    }
}
