using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Playwright;

namespace MijnKeuken.Web.Tests;

/// <summary>
/// Assembly-level fixture that starts the web application once for all tests
/// and provides a shared Playwright browser instance.
/// </summary>
[SetUpFixture]
public class WebAppFixture
{
    private Process? _serverProcess;

    public static IBrowser Browser { get; private set; } = null!;
    public static IPlaywright PlaywrightInstance { get; private set; } = null!;
    public static string BaseUrl { get; private set; } = null!;

    /// <summary>Pre-registered, auto-approved seed user credentials for login tests.</summary>
    public static string SeedUsername { get; } = "test_admin";
    public static string SeedPassword { get; } = "Test1234!";

    private static readonly string SolutionRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string WebProjectPath = Path.Combine(SolutionRoot,
        "src", "MijnKeuken.Web");

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var port = Random.Shared.Next(5100, 5900);
        BaseUrl = $"http://localhost:{port}";

        // Build the web project first
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{WebProjectPath}\" -c Debug -q",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        buildProcess.Start();
        await buildProcess.WaitForExitAsync();

        var dll = Path.Combine(WebProjectPath, "bin", "Debug", "net10.0", "MijnKeuken.Web.dll");
        if (!File.Exists(dll))
            throw new FileNotFoundException($"Built dll not found at {dll}");

        // Read CONNECTION_STRING_4_TESTS from environment or launchSettings.json
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_4_TESTS");
        if (string.IsNullOrEmpty(connectionString))
            connectionString = ReadConnectionStringFromLaunchSettings();

        // Clear existing users so the seed user will be the first (auto-approved)
        if (!string.IsNullOrEmpty(connectionString))
            await ClearTablesAsync(connectionString);

        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{dll}\" --urls {BaseUrl}",
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = WebProjectPath
            }
        };

        _serverProcess.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        if (!string.IsNullOrEmpty(connectionString))
            _serverProcess.StartInfo.Environment["CONNECTION_STRING"] = connectionString;

        _serverProcess.Start();

        // Wait for the server to be ready
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        for (var i = 0; i < 30; i++)
        {
            try
            {
                await httpClient.GetAsync(BaseUrl);
                break;
            }
            catch
            {
                if (_serverProcess.HasExited)
                    throw new Exception($"Web app process exited with code {_serverProcess.ExitCode}");
                if (i == 29)
                    throw new Exception($"Web application failed to start at {BaseUrl}");
                await Task.Delay(1000);
            }
        }

        // Register seed user via API (first user = auto-approved)
        var registerResponse = await httpClient.PostAsJsonAsync(
            $"{BaseUrl}/api/auth/register",
            new { Username = SeedUsername, Password = SeedPassword, Email = "admin@test.nl" });

        if (!registerResponse.IsSuccessStatusCode)
        {
            var body = await registerResponse.Content.ReadAsStringAsync();
            throw new Exception($"Failed to register seed user: {registerResponse.StatusCode} - {body}");
        }

        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (Browser is not null)
            await Browser.CloseAsync();

        PlaywrightInstance?.Dispose();

        if (_serverProcess is not null && !_serverProcess.HasExited)
        {
            _serverProcess.Kill(entireProcessTree: true);
            _serverProcess.Dispose();
        }
    }

    private static string? ReadConnectionStringFromLaunchSettings()
    {
        var path = Path.Combine(WebProjectPath, "Properties", "launchSettings.json");
        if (!File.Exists(path)) return null;

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (doc.RootElement.TryGetProperty("profiles", out var profiles))
        {
            foreach (var profile in profiles.EnumerateObject())
            {
                if (profile.Value.TryGetProperty("environmentVariables", out var envVars)
                    && envVars.TryGetProperty("CONNECTION_STRING_4_TESTS", out var connStr))
                {
                    return connStr.GetString();
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Clears the Users and Tags tables so tests start with a clean state.
    /// Safe to call even if the tables don't exist yet (migrations haven't run).
    /// </summary>
    private static async Task ClearTablesAsync(string connectionString)
    {
        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                IF OBJECT_ID('Tags', 'U') IS NOT NULL DELETE FROM Tags;
                IF OBJECT_ID('Users', 'U') IS NOT NULL DELETE FROM Users;
                """;
            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // Database or table may not exist yet (first run); migrations will create it
        }
    }
}

/// <summary>
/// Base class for Playwright UI tests. Provides convenience methods
/// using the shared WebAppFixture browser and server.
/// </summary>
public abstract class PlaywrightTestBase
{
    protected static string BaseUrl => WebAppFixture.BaseUrl;
    protected static IBrowser Browser => WebAppFixture.Browser;

    protected async Task<IBrowserContext> CreateContextAsync()
    {
        return await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
    }

    /// <summary>
    /// Navigates to a URL and waits for the Blazor Server circuit to fully establish.
    /// Blazor SSR renders static HTML first, then hydrates via SignalR.
    /// We must wait for the circuit before interacting with forms.
    /// </summary>
    protected static async Task GotoAndWaitForBlazorAsync(IPage page, string url)
    {
        await page.GotoAsync(url);
        await page.WaitForLoadStateAsync();

        // Wait for the Blazor Server circuit to connect and stabilize.
        // The circuit typically establishes within 1-2 seconds after page load.
        await page.WaitForTimeoutAsync(3000);
    }
}
