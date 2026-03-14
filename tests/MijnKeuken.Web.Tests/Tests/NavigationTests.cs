namespace MijnKeuken.Web.Tests.Tests;

/// <summary>
/// Tests for navigation: sidebar links, top bar buttons, and page accessibility when authenticated.
/// </summary>
[TestFixture]
public class NavigationTests : PlaywrightTestBase
{
    /// <summary>
    /// Logs in using the pre-registered seed user from WebAppFixture.
    /// </summary>
    private async Task<Microsoft.Playwright.IPage> LoginAsSeedUserAsync(Microsoft.Playwright.IBrowserContext context)
    {
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        // Wait for home page content to confirm login succeeded
        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        return page;
    }

    /// <summary>
    /// When authenticated, the sidebar should show navigation links (hover to expand mini drawer).
    /// </summary>
    [Test]
    public async Task AuthenticatedUser_SeesNavigationLinks()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Hover the mini drawer to expand it and reveal link text
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.That(await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Dashboard" }).IsVisibleAsync(), Is.True);
        Assert.That(await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Counter" }).IsVisibleAsync(), Is.True);
        Assert.That(await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Weer" }).IsVisibleAsync(), Is.True);
        Assert.That(await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Gebruikers" }).IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// The top bar should show the app title when authenticated.
    /// </summary>
    [Test]
    public async Task AuthenticatedUser_SeesTopBarIcons()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Top bar should contain the app title
        var appTitle = page.Locator(".mud-appbar").GetByText("MijnKeuken");
        Assert.That(await appTitle.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Clicking the Counter link should navigate to the counter page.
    /// </summary>
    [Test]
    public async Task NavigateToCounter_ShowsCounterPage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Hover mini drawer to expand, then click Counter link
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Counter" }).ClickAsync();

        var counterTitle = page.GetByText("Counter").First;
        await counterTitle.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await counterTitle.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Clicking the Weer link should navigate to the weather page.
    /// </summary>
    [Test]
    public async Task NavigateToWeather_ShowsWeatherPage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Hover mini drawer to expand, then click Weer link
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Weer" }).ClickAsync();

        var weatherTitle = page.GetByText("Weer").First;
        await weatherTitle.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await weatherTitle.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Clicking the Gebruikers link should navigate to the users page.
    /// </summary>
    [Test]
    public async Task NavigateToUsers_ShowsUsersPage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Hover mini drawer to expand, then click Gebruikers link
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Gebruikers" }).ClickAsync();

        var usersTitle = page.GetByText("Gebruikers").First;
        await usersTitle.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await usersTitle.IsVisibleAsync(), Is.True);
    }
}
