using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Ingredients.DTOs;
using MijnKeuken.Web.Auth;

namespace MijnKeuken.Web.Services;

public class IngredientService(
    IHttpClientFactory httpClientFactory,
    NavigationManager nav,
    JwtAuthenticationStateProvider authStateProvider) : IIngredientService
{
    private record ErrorResponse(string Error);
    private record CreateResponse(Guid Id);
    private record ScrapeRequest(string Url);

    public async Task<List<IngredientDto>> GetAllAsync()
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<IngredientDto>>("api/ingredients") ?? [];
    }

    public async Task<Result<Guid>> CreateAsync(CreateIngredientRequest request)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/ingredients", request);

        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
            return Result<Guid>.Success(created!.Id);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result<Guid>.Failure(error?.Error ?? "Ingrediënt aanmaken mislukt.");
    }

    public async Task<Result> UpdateAsync(Guid id, CreateIngredientRequest request)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"api/ingredients/{id}", request);

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Ingrediënt bijwerken mislukt.");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"api/ingredients/{id}");

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Ingrediënt verwijderen mislukt.");
    }

    public async Task<Result<ScrapedIngredientDto>> ScrapeFromUrlAsync(string url)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/ingredients/scrape", new ScrapeRequest(url));

        if (response.IsSuccessStatusCode)
        {
            var scraped = await response.Content.ReadFromJsonAsync<ScrapedIngredientDto>();
            return scraped is not null
                ? Result<ScrapedIngredientDto>.Success(scraped)
                : Result<ScrapedIngredientDto>.Failure("Geen gegevens ontvangen.");
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result<ScrapedIngredientDto>.Failure(error?.Error ?? "Scrapen mislukt.");
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
