using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using MijnKeuken.Application.Dashboard.DTOs;
using MijnKeuken.Web.Auth;

namespace MijnKeuken.Web.Services;

public class DashboardService(
    IHttpClientFactory httpClientFactory,
    NavigationManager nav,
    JwtAuthenticationStateProvider authStateProvider) : IDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(int top = 5)
    {
        using var client = CreateClient();
        return await client.GetFromJsonAsync<DashboardStatsDto>($"api/dashboard/stats?top={top}")
            ?? new DashboardStatsDto([], [], null);
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
