using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.Tags.DTOs;
using MijnKeuken.Domain.Entities;
using MijnKeuken.Web.Auth;

namespace MijnKeuken.Web.Services;

public class TagService(
    IHttpClientFactory httpClientFactory,
    NavigationManager nav,
    JwtAuthenticationStateProvider authStateProvider) : ITagService
{
    private record ErrorResponse(string Error);
    private record CreateResponse(Guid Id);

    public async Task<List<TagDto>> GetAllAsync()
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<TagDto>>("api/tags") ?? [];
    }

    public async Task<Result<Guid>> CreateAsync(string name, TagType type, string color)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/tags", new { name, type, color });

        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
            return Result<Guid>.Success(created!.Id);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result<Guid>.Failure(error?.Error ?? "Tag aanmaken mislukt.");
    }

    public async Task<Result> UpdateAsync(Guid id, string name, TagType type, string color)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"api/tags/{id}", new { name, type, color });

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Tag bijwerken mislukt.");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"api/tags/{id}");

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Tag verwijderen mislukt.");
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
