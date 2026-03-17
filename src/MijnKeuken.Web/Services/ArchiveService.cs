using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MijnKeuken.Application.Archive.DTOs;
using MijnKeuken.Application.Common;
using MijnKeuken.Web.Auth;

namespace MijnKeuken.Web.Services;

public class ArchiveService(
    IHttpClientFactory httpClientFactory,
    NavigationManager nav,
    JwtAuthenticationStateProvider authStateProvider) : IArchiveService
{
    private record ErrorResponse(string Error);

    public async Task<List<ArchivedItemDto>> GetAllAsync()
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<ArchivedItemDto>>("api/archive") ?? [];
    }

    public async Task<Result> UnarchiveRecipeAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.PatchAsync($"api/archive/recipes/{id}/unarchive", null);

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Recept herstellen mislukt.");
    }

    public async Task<Result> UnarchiveIngredientAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.PatchAsync($"api/archive/ingredients/{id}/unarchive", null);

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Ingrediënt herstellen mislukt.");
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
