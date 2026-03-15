namespace MijnKeuken.Web.Tests.Tests;

/// <summary>
/// Tests for the Storage Locations management page: CRUD operations and search.
/// </summary>
[TestFixture]
public class StorageLocationTests : PlaywrightTestBase
{
    private async Task<Microsoft.Playwright.IPage> LoginAndNavigateToStorageAsync(Microsoft.Playwright.IBrowserContext context)
    {
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        // Navigate to Storage page via drawer
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Opslag" }).ClickAsync();

        // Wait for the table to be present (page + data loaded)
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        return page;
    }

    private static async Task CreateLocationAsync(Microsoft.Playwright.IPage page, string name, string description = "")
    {
        await page.Locator("button:has-text('Nieuwe locatie')").ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        await dialog.GetByLabel("Naam").FillAsync(name);

        if (!string.IsNullOrEmpty(description))
            await dialog.GetByLabel("Omschrijving").FillAsync(description);

        await dialog.Locator("button:has-text('Aanmaken')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);
    }

    [Test]
    public async Task StoragePage_DisplaysTitleAndCreateButton()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToStorageAsync(context);

        Assert.That(await page.Locator("h4").GetByText("Opslaglocaties").IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("button:has-text('Nieuwe locatie')").IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task CreateLocation_AddsToTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToStorageAsync(context);

        var name = $"Loc_{Guid.NewGuid():N}"[..14];

        await CreateLocationAsync(page, name, "Test omschrijving");

        var cell = page.Locator("td[data-label='Naam']", new() { HasTextString = name });
        await cell.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await cell.IsVisibleAsync(), Is.True);

        // Description should also be visible
        var descCell = page.Locator("td[data-label='Omschrijving']", new() { HasTextString = "Test omschrijving" });
        Assert.That(await descCell.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task EditLocation_UpdatesInTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToStorageAsync(context);

        var originalName = $"Edit_{Guid.NewGuid():N}"[..14];
        var updatedName = $"Upd_{Guid.NewGuid():N}"[..14];

        await CreateLocationAsync(page, originalName);

        // Click the edit button on the row
        var row = page.Locator("tr", new() { HasText = originalName });
        await row.Locator("button").Nth(0).ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        var nameField = dialog.GetByLabel("Naam");
        await nameField.ClearAsync();
        await nameField.FillAsync(updatedName);

        await dialog.Locator("button:has-text('Opslaan')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        var updatedCell = page.Locator("td[data-label='Naam']", new() { HasTextString = updatedName });
        Assert.That(await updatedCell.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task DeleteLocation_RemovesFromTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToStorageAsync(context);

        var name = $"Del_{Guid.NewGuid():N}"[..14];

        await CreateLocationAsync(page, name);

        var cell = page.Locator("td[data-label='Naam']", new() { HasTextString = name });
        await cell.WaitForAsync(new() { Timeout = 5000 });

        // Click the delete button (second button in row)
        var row = page.Locator("tr", new() { HasText = name });
        await row.Locator("button").Nth(1).ClickAsync();

        // Confirm deletion
        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });
        await dialog.Locator("button:has-text('Verwijderen')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        Assert.That(await cell.IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task SearchByName_FiltersTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToStorageAsync(context);

        var uniquePrefix = Guid.NewGuid().ToString("N")[..6];
        var matchName = $"{uniquePrefix}_Match";
        var otherName = $"Other_{Guid.NewGuid():N}"[..14];

        await CreateLocationAsync(page, matchName);
        await CreateLocationAsync(page, otherName);

        var matchCell = page.Locator("td[data-label='Naam']", new() { HasTextString = matchName });
        Assert.That(await matchCell.IsVisibleAsync(), Is.True);

        // Search by the unique prefix
        await page.GetByPlaceholder("Zoeken op naam...").FillAsync(uniquePrefix);
        await page.WaitForTimeoutAsync(500);

        Assert.That(await matchCell.IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("tr", new() { HasText = otherName }).IsVisibleAsync(), Is.False);
    }
}
