using MijnKeuken.Web.Tests.Helpers;

namespace MijnKeuken.Web.Tests;

/// <summary>
/// Tests for dark mode: toggle button, theme application, and persistence across sessions.
/// </summary>
[TestFixture]
public class DarkModeTests : PlaywrightTestBase
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

        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        return page;
    }

    /// <summary>
    /// The top bar should display a dark mode toggle button when authenticated.
    /// </summary>
    [Test]
    public async Task AuthenticatedUser_SeesDarkModeToggle()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        var toggle = page.GetByLabel("Thema wisselen");
        Assert.That(await toggle.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Clicking the dark mode toggle should switch the MudThemeProvider to dark mode,
    /// which changes the body background to a dark color.
    /// </summary>
    [Test]
    public async Task ClickingToggle_SwitchesToDarkMode()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Wait for theme preference to load from profile
        await page.WaitForTimeoutAsync(2000);

        // Capture background color before toggle
        var bgBefore = await page.EvaluateAsync<string>(
            "window.getComputedStyle(document.body).backgroundColor");

        await page.GetByLabel("Thema wisselen").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        // Capture background color after toggle
        var bgAfter = await page.EvaluateAsync<string>(
            "window.getComputedStyle(document.body).backgroundColor");

        Assert.That(bgAfter, Is.Not.EqualTo(bgBefore),
            "Background color should change after toggling dark mode");
    }

    /// <summary>
    /// Clicking the toggle twice should return to the original theme.
    /// </summary>
    [Test]
    public async Task ClickingToggleTwice_ReturnsToOriginalMode()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Wait for theme preference to load from profile
        await page.WaitForTimeoutAsync(2000);

        var bgOriginal = await page.EvaluateAsync<string>(
            "window.getComputedStyle(document.body).backgroundColor");

        // Toggle on then off
        await page.GetByLabel("Thema wisselen").ClickAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByLabel("Thema wisselen").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        var bgAfterDoubleToggle = await page.EvaluateAsync<string>(
            "window.getComputedStyle(document.body).backgroundColor");

        Assert.That(bgAfterDoubleToggle, Is.EqualTo(bgOriginal),
            "Background should return to original after toggling twice");
    }

    /// <summary>
    /// Dark mode preference should persist: toggle dark mode, log out, log back in,
    /// and the theme should still be dark.
    /// </summary>
    [Test]
    public async Task DarkModePreference_PersistsAcrossLogin()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Enable dark mode
        await page.GetByLabel("Thema wisselen").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        var darkBg = await page.EvaluateAsync<string>(
            "window.getComputedStyle(document.body).backgroundColor");

        // Log out by clicking the last icon button in the app bar (logout)
        var logoutButton = page.Locator(".mud-appbar button").Last;
        await logoutButton.ClickAsync();

        // Wait for login page
        await page.GetByText("Inloggen").First.WaitForAsync(new() { Timeout = 10000 });

        // Log back in
        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();
        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        // Wait for theme to load from profile
        await page.WaitForTimeoutAsync(2000);

        var bgAfterRelogin = await page.EvaluateAsync<string>(
            "window.getComputedStyle(document.body).backgroundColor");

        Assert.That(bgAfterRelogin, Is.EqualTo(darkBg),
            "Dark mode should persist after logging out and back in");

        // Clean up: toggle back to light mode for other tests
        await page.GetByLabel("Thema wisselen").ClickAsync();
        await page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// The profile page should display the current theme preference.
    /// </summary>
    [Test]
    public async Task ProfilePage_ShowsThemePreference()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Click the profile icon button (second to last in app bar)
        await page.Locator("a[href='/account/profile']").ClickAsync();

        var themeText = page.GetByText("Thema:");
        await themeText.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await themeText.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Toggling dark mode while the profile page is open should immediately
    /// update the displayed theme preference without a page reload.
    /// </summary>
    [Test]
    public async Task ProfilePage_ThemeUpdatesImmediatelyOnToggle()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsSeedUserAsync(context);

        // Navigate to profile page
        await page.Locator("a[href='/account/profile']").ClickAsync();
        await page.GetByText("Thema:").WaitForAsync(new() { Timeout = 10000 });

        // Read current theme state from the profile
        var themaLine = page.Locator("text=Thema:").Locator("..");
        var initialText = await themaLine.TextContentAsync();
        var startsLight = initialText!.Contains("Licht");

        var expectedAfterToggle = startsLight ? "Donker" : "Licht";
        var expectedAfterRevert = startsLight ? "Licht" : "Donker";

        // Toggle theme from the top bar
        await page.GetByLabel("Thema wisselen").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        // Profile should immediately reflect the change
        Assert.That(await themaLine.TextContentAsync(), Does.Contain(expectedAfterToggle));

        // Toggle back and verify it reverts
        await page.GetByLabel("Thema wisselen").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.That(await themaLine.TextContentAsync(), Does.Contain(expectedAfterRevert));
    }
}
