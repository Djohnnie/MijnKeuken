using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace MijnKeuken.Web.Auth;

public class JwtAuthenticationStateProvider(IJSRuntime jsRuntime) : AuthenticationStateProvider
{
    private const string StorageKey = "auth_token";
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());
    private bool _initialized;

    public string? CurrentToken { get; private set; }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_currentUser));

    /// <summary>
    /// Loads the JWT token from localStorage on first call. If the token is
    /// valid and not expired, the authentication state is restored.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var token = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(token) && IsTokenValid(token))
                SetToken(token);
        }
        catch
        {
            // JS interop may not be available yet during pre-render
        }
    }

    public async Task SetTokenAsync(string token)
    {
        SetToken(token);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, token);
    }

    public async Task ClearTokenAsync()
    {
        ClearToken();
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }

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

    private static bool IsTokenValid(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }
}
