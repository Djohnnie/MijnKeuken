using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Users.DTOs;
using MijnKeuken.Web.Auth;

namespace MijnKeuken.Web.Services;

public class UserService(
    IHttpClientFactory httpClientFactory,
    NavigationManager nav,
    JwtAuthenticationStateProvider authStateProvider) : IUserService
{
    private record ErrorResponse(string Error);

    public async Task<Result<UserProfileDto>> GetProfileAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("api/users/profile");

        if (response.IsSuccessStatusCode)
        {
            var dto = await response.Content.ReadFromJsonAsync<UserProfileDto>();
            return Result<UserProfileDto>.Success(dto!);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result<UserProfileDto>.Failure(error?.Error ?? "Profiel ophalen mislukt.");
    }

    public async Task<List<UserListItemDto>> GetAllUsersAsync()
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<UserListItemDto>>("api/users") ?? [];
    }

    public async Task<List<PendingUserDto>> GetPendingUsersAsync()
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<PendingUserDto>>("api/users/pending") ?? [];
    }

    public async Task<Result> ApproveUserAsync(Guid targetUserId)
    {
        using var client = CreateClient();
        var response = await client.PostAsync($"api/users/{targetUserId}/approve", null);

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Goedkeuring mislukt.");
    }

    private HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(nav.BaseUri);

        if (authStateProvider.CurrentToken is not null)
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authStateProvider.CurrentToken);

        return client;
    }
}
