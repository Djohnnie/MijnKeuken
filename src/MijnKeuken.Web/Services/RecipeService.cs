using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Recipes.DTOs;
using MijnKeuken.Web.Auth;

namespace MijnKeuken.Web.Services;

public class RecipeService(
    IHttpClientFactory httpClientFactory,
    NavigationManager nav,
    JwtAuthenticationStateProvider authStateProvider) : IRecipeService
{
    private record ErrorResponse(string Error);
    private record CreateResponse(Guid Id);
    private record ScrapeRequest(string Url);

    public async Task<List<RecipeDto>> GetAllAsync()
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<RecipeDto>>("api/recipes") ?? [];
    }

    public async Task<RecipeDto?> GetByIdAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"api/recipes/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<RecipeDto>();
    }

    public async Task<Result<Guid>> CreateAsync(CreateRecipeRequest request)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/recipes", request);

        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
            return Result<Guid>.Success(created!.Id);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result<Guid>.Failure(error?.Error ?? "Recept aanmaken mislukt.");
    }

    public async Task<Result> UpdateAsync(Guid id, CreateRecipeRequest request)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"api/recipes/{id}", request);

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Recept bijwerken mislukt.");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"api/recipes/{id}");

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Recept verwijderen mislukt.");
    }

    public async Task<Result<ScrapedRecipeDto>> ScrapeFromUrlAsync(string url)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/recipes/scrape", new ScrapeRequest(url));

        if (response.IsSuccessStatusCode)
        {
            var scraped = await response.Content.ReadFromJsonAsync<ScrapedRecipeDto>();
            return scraped is not null
                ? Result<ScrapedRecipeDto>.Success(scraped)
                : Result<ScrapedRecipeDto>.Failure("Geen gegevens ontvangen.");
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result<ScrapedRecipeDto>.Failure(error?.Error ?? "Scrapen mislukt.");
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
