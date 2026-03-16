using MijnKeuken.Web.Tests.Helpers;

namespace MijnKeuken.Web.Tests;

/// <summary>
/// Tests for the Ingredients management page: CRUD operations, filtering, and search.
/// </summary>
[TestFixture]
public class IngredientTests : PlaywrightTestBase
{
    private async Task<Microsoft.Playwright.IPage> LoginAndNavigateToIngredientsAsync(Microsoft.Playwright.IBrowserContext context)
    {
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        // Navigate to Ingredients page via drawer
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Ingrediënten" }).ClickAsync();

        // Wait for the Ingredients table to be present
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        return page;
    }

    private static async Task CreateIngredientAsync(Microsoft.Playwright.IPage page, string title, string description = "")
    {
        await page.Locator("button:has-text('Nieuw ingrediënt')").ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        await dialog.GetByLabel("Titel").FillAsync(title);
        if (!string.IsNullOrEmpty(description))
            await dialog.GetByLabel("Omschrijving").FillAsync(description);

        await dialog.Locator("button:has-text('Aanmaken')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);
    }

    [Test]
    public async Task IngredientsPage_DisplaysTitleAndCreateButton()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToIngredientsAsync(context);

        Assert.That(await page.Locator("h4").GetByText("Ingrediënten").IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("button:has-text('Nieuw ingrediënt')").IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task CreateIngredient_AddsToTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToIngredientsAsync(context);

        var title = $"Ing_{Guid.NewGuid():N}"[..14];

        await CreateIngredientAsync(page, title, "Test omschrijving");

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await titleCell.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task EditIngredient_UpdatesInTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToIngredientsAsync(context);

        var originalTitle = $"Edit_{Guid.NewGuid():N}"[..14];
        var updatedTitle = $"Upd_{Guid.NewGuid():N}"[..14];

        await CreateIngredientAsync(page, originalTitle);

        // Click the edit button on the row
        var row = page.Locator("tr", new() { HasText = originalTitle });
        await row.Locator("button").Nth(0).ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        var titleField = dialog.GetByLabel("Titel");
        await titleField.ClearAsync();
        await titleField.FillAsync(updatedTitle);

        await dialog.Locator("button:has-text('Opslaan')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        var updatedCell = page.Locator("td[data-label='Titel']", new() { HasTextString = updatedTitle });
        Assert.That(await updatedCell.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task DeleteIngredient_RemovesFromTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToIngredientsAsync(context);

        var title = $"Del_{Guid.NewGuid():N}"[..14];

        await CreateIngredientAsync(page, title);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 5000 });

        // Click the delete button (second icon button in the row)
        var row = page.Locator("tr", new() { HasText = title });
        await row.Locator("button").Nth(1).ClickAsync();

        // Confirm deletion
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
        var page = await LoginAndNavigateToIngredientsAsync(context);

        var uniquePrefix = Guid.NewGuid().ToString("N")[..6];
        var matchTitle = $"{uniquePrefix}_Match";
        var otherTitle = $"Other_{Guid.NewGuid():N}"[..14];

        await CreateIngredientAsync(page, matchTitle);
        await CreateIngredientAsync(page, otherTitle);

        var matchCell = page.Locator("td[data-label='Titel']", new() { HasTextString = matchTitle });
        Assert.That(await matchCell.IsVisibleAsync(), Is.True);

        // Type the unique prefix in the search box
        await page.GetByPlaceholder("Zoeken op titel...").FillAsync(uniquePrefix);
        await page.WaitForTimeoutAsync(500);

        Assert.That(await matchCell.IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("tr", new() { HasText = otherTitle }).IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task PercentageUnit_SetsTotalTo100AndReadOnly()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToIngredientsAsync(context);

        await page.Locator("button:has-text('Nieuw ingrediënt')").ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        // Select "Percentage" unit
        await dialog.Locator("div.mud-select[id]").First.ClickAsync();
        await page.WaitForTimeoutAsync(500);
        await page.Locator(".mud-popover-open .mud-list-item").GetByText("Percentage", new() { Exact = true }).ClickAsync();
        await page.WaitForTimeoutAsync(500);

        // Total should be set to 100
        var totalInput = dialog.GetByLabel("Totaal");
        var totalValue = await totalInput.InputValueAsync();
        Assert.That(totalValue, Is.EqualTo("100"));

        // Total input should be readonly
        var readonlyAttr = await totalInput.GetAttributeAsync("readonly");
        Assert.That(readonlyAttr, Is.Not.Null);

        // Switch back to "Stuks" — total should become editable again
        await dialog.Locator("div.mud-select[id]").First.ClickAsync();
        await page.WaitForTimeoutAsync(500);
        await page.Locator(".mud-popover-open .mud-list-item").GetByText("Stuks", new() { Exact = true }).ClickAsync();
        await page.WaitForTimeoutAsync(500);

        readonlyAttr = await totalInput.GetAttributeAsync("readonly");
        Assert.That(readonlyAttr, Is.Null);

        await dialog.Locator("button:has-text('Annuleren')").ClickAsync();
    }
}
