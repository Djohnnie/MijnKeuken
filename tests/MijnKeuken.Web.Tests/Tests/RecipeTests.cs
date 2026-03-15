namespace MijnKeuken.Web.Tests.Tests;

/// <summary>
/// Tests for the Recipes management: list page, form page, and search.
/// </summary>
[TestFixture]
public class RecipeTests : PlaywrightTestBase
{
    private async Task<Microsoft.Playwright.IPage> LoginAndNavigateToRecipesAsync(Microsoft.Playwright.IBrowserContext context)
    {
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Recepten" }).ClickAsync();

        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        return page;
    }

    private static async Task CreateRecipeViaFormAsync(Microsoft.Playwright.IPage page, string title, string description = "")
    {
        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();

        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        await page.GetByLabel("Titel").FillAsync(title);
        if (!string.IsNullOrEmpty(description))
            await page.GetByLabel("Omschrijving").FillAsync(description);

        await page.Locator("button:has-text('Aanmaken')").ClickAsync();

        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task RecipesPage_DisplaysTitleAndCreateButton()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        Assert.That(await page.Locator("h4").GetByText("Recepten").IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("button:has-text('Nieuw recept')").IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task CreateRecipe_NavigatesToFormAndAddsToTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        var title = $"Rec_{Guid.NewGuid():N}"[..14];

        await CreateRecipeViaFormAsync(page, title, "Test omschrijving");

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await titleCell.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task EditRecipe_NavigatesToFormAndUpdatesTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        var originalTitle = $"Edit_{Guid.NewGuid():N}"[..14];
        var updatedTitle = $"Upd_{Guid.NewGuid():N}"[..14];

        await CreateRecipeViaFormAsync(page, originalTitle);

        // Click the edit button on the row
        var row = page.Locator("tr", new() { HasText = originalTitle });
        await row.Locator("button").Nth(0).ClickAsync();

        // We should be on the edit form page now
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        var titleField = page.GetByLabel("Titel");
        await titleField.ClearAsync();
        await titleField.FillAsync(updatedTitle);

        await page.Locator("button:has-text('Opslaan')").ClickAsync();

        // Should navigate back to recipes list
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        var updatedCell = page.Locator("td[data-label='Titel']", new() { HasTextString = updatedTitle });
        await updatedCell.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await updatedCell.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task DeleteRecipe_RemovesFromTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        var title = $"Del_{Guid.NewGuid():N}"[..14];

        await CreateRecipeViaFormAsync(page, title);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 5000 });

        var row = page.Locator("tr", new() { HasText = title });
        await row.Locator("button").Nth(1).ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });
        await dialog.Locator("button:has-text('Verwijderen')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        Assert.That(await titleCell.IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task SearchByTitle_FiltersTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        var uniquePrefix = Guid.NewGuid().ToString("N")[..6];
        var matchTitle = $"{uniquePrefix}_Match";
        var otherTitle = $"Other_{Guid.NewGuid():N}"[..14];

        await CreateRecipeViaFormAsync(page, matchTitle);
        await CreateRecipeViaFormAsync(page, otherTitle);

        var matchCell = page.Locator("td[data-label='Titel']", new() { HasTextString = matchTitle });
        Assert.That(await matchCell.IsVisibleAsync(), Is.True);

        await page.GetByPlaceholder("Zoeken op titel...").FillAsync(uniquePrefix);
        await page.WaitForTimeoutAsync(500);

        Assert.That(await matchCell.IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("tr", new() { HasText = otherTitle }).IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task MarkdownPreview_ShowsRenderedContent()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();

        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        await page.GetByPlaceholder("Schrijf je bereidingswijze in Markdown...").FillAsync("## Stap 1\nKook de pasta");
        await page.WaitForTimeoutAsync(300);

        await page.GetByText("Voorbeeld").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        var preview = page.Locator(".markdown-preview");
        var previewHtml = await preview.InnerHTMLAsync();
        Assert.That(previewHtml, Does.Contain("Stap 1"));
        Assert.That(previewHtml, Does.Contain("Kook de pasta"));

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }
}
