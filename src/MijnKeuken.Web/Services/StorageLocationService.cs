using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MijnKeuken.Application.Common;
using MijnKeuken.Application.StorageLocations.DTOs;
using MijnKeuken.Web.Auth;

namespace MijnKeuken.Web.Services;

public class StorageLocationService(
    IHttpClientFactory httpClientFactory,
    NavigationManager nav,
    JwtAuthenticationStateProvider authStateProvider) : IStorageLocationService
{
    private record ErrorResponse(string Error);
    private record CreateResponse(Guid Id);

    public async Task<List<StorageLocationDto>> GetAllAsync()
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<List<StorageLocationDto>>("api/storage-locations") ?? [];
    }

    public async Task<Result<Guid>> CreateAsync(string name, string description)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("api/storage-locations", new { name, description });

        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<CreateResponse>();
            return Result<Guid>.Success(created!.Id);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result<Guid>.Failure(error?.Error ?? "Opslaglocatie aanmaken mislukt.");
    }

    public async Task<Result> UpdateAsync(Guid id, string name, string description)
    {
        using var client = CreateClient();
        var response = await client.PutAsJsonAsync($"api/storage-locations/{id}", new { name, description });

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Opslaglocatie bijwerken mislukt.");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync($"api/storage-locations/{id}");

        if (response.IsSuccessStatusCode)
            return Result.Success();

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return Result.Failure(error?.Error ?? "Opslaglocatie verwijderen mislukt.");
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
