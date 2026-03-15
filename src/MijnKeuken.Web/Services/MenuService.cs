using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Menu.DTOs;
using MijnKeuken.Web.Auth;

namespace MijnKeuken.Web.Services;

public class MenuService(
    IHttpClientFactory httpClientFactory,
    NavigationManager nav,
    JwtAuthenticationStateProvider authStateProvider) : IMenuService
{
    private record ErrorResponse(string Error);
    private record UpsertResponse(Guid Id);

    public async Task<List<MenuEntryDto>> GetByRangeAsync(DateOnly from, DateOnly to)
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<MenuEntryDto>>(
            $"api/menu?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}") ?? [];
    }

    public async Task<Result<Guid>> UpsertAsync(UpsertMenuEntryRequest request)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync("api/menu", request);

        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<UpsertResponse>();
            return Result<Guid>.Success(created!.Id);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result<Guid>.Failure(error?.Error ?? "Menu-item opslaan mislukt.");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"api/menu/{id}");

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Menu-item verwijderen mislukt.");
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
