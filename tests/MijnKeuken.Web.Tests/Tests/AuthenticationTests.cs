namespace MijnKeuken.Web.Tests.Tests;

/// <summary>
/// Tests for authentication flows: login page redirect, login, registration, and logout.
/// </summary>
[TestFixture]
public class AuthenticationTests : PlaywrightTestBase
{
    /// <summary>
    /// Unauthenticated users visiting the root should be redirected to the login page.
    /// </summary>
    [Test]
    public async Task UnauthenticatedUser_IsRedirectedToLogin()
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, BaseUrl);

        var loginTitle = page.GetByText("Inloggen").First;
        await loginTitle.WaitForAsync(new() { Timeout = 15000 });
        Assert.That(await loginTitle.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// The login page should display the title and the form fields.
    /// </summary>
    [Test]
    public async Task LoginPage_DisplaysFormFields()
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        var title = page.GetByText("Inloggen").First;
        await title.WaitForAsync();
        Assert.That(await title.IsVisibleAsync(), Is.True);

        Assert.That(await page.GetByLabel("Gebruikersnaam").IsVisibleAsync(), Is.True);
        Assert.That(await page.GetByLabel("Wachtwoord").IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// The login page should show a link to the registration page.
    /// </summary>
    [Test]
    public async Task LoginPage_HasLinkToRegister()
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        var registerLink = page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Registreren" });
        Assert.That(await registerLink.IsVisibleAsync(), Is.True);

        await registerLink.ClickAsync();

        var registerTitle = page.GetByText("Registreren").First;
        await registerTitle.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await registerTitle.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// The registration page should display the form with all required fields.
    /// </summary>
    [Test]
    public async Task RegisterPage_DisplaysFormFields()
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/register");

        var title = page.GetByText("Registreren").First;
        await title.WaitForAsync();
        Assert.That(await title.IsVisibleAsync(), Is.True);

        Assert.That(await page.GetByLabel("Gebruikersnaam").IsVisibleAsync(), Is.True);
        Assert.That(await page.GetByLabel("E-mailadres").IsVisibleAsync(), Is.True);

        var passwordFields = page.GetByLabel("Wachtwoord", new() { Exact = false });
        Assert.That(await passwordFields.CountAsync(), Is.GreaterThanOrEqualTo(2));
    }

    /// <summary>
    /// Registering a new user should succeed and show a success message.
    /// </summary>
    [Test]
    public async Task Register_NewUser_ShowsSuccess()
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();

        var username = $"regtest_{Guid.NewGuid():N}"[..20];

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/register");

        await page.GetByLabel("Gebruikersnaam").FillAsync(username);
        await page.GetByLabel("E-mailadres").FillAsync($"{username}@test.nl");
        await page.GetByLabel("Wachtwoord", new() { Exact = true }).FillAsync("Test1234!");
        await page.GetByLabel("Wachtwoord bevestigen").FillAsync("Test1234!");

        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Registreren" }).ClickAsync();

        var successAlert = page.GetByText("Registratie geslaagd!");
        await successAlert.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await successAlert.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// After logging in with valid credentials, the user should see the home page.
    /// Uses the pre-registered seed user (auto-approved) from WebAppFixture.
    /// </summary>
    [Test]
    public async Task Login_WithValidCredentials_ShowsHomePage()
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        var welcomeText = page.GetByText("Welkom bij MijnKeuken");
        await welcomeText.WaitForAsync(new() { Timeout = 15000 });
        Assert.That(await welcomeText.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Login with wrong credentials should show an error message.
    /// </summary>
    [Test]
    public async Task Login_WithInvalidCredentials_ShowsError()
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync("nonexistent");
        await page.GetByLabel("Wachtwoord").FillAsync("WrongPassword!");
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        // MudAlert with Variant.Outlined uses mud-alert-outlined-error class
        var errorAlert = page.Locator(".mud-alert-outlined-error");
        await errorAlert.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await errorAlert.IsVisibleAsync(), Is.True);
    }
}
