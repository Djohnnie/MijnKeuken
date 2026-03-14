using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MijnKeuken.Application.Common;
using MijnKeuken.Web.Auth;

namespace MijnKeuken.Web.Services;

public class AuthService(
    IHttpClientFactory httpClientFactory,
    NavigationManager nav) : IAuthService
{
    private record LoginResponse(string Token);
    private record ErrorResponse(string Error);

    public async Task<Result> RegisterAsync(string username, string password, string email)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/auth/register",
            new { username, password, email });

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Registratie mislukt.");
    }

    public async Task<Result<string>> LoginAsync(string username, string password)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/auth/login",
            new { username, password });

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return Result<string>.Success(data!.Token);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result<string>.Failure(error?.Error ?? "Inloggen mislukt.");
    }

    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(nav.BaseUri);
        return client;
    }
}
