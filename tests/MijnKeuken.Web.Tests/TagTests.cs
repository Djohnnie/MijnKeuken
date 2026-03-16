using MijnKeuken.Web.Tests.Helpers;

namespace MijnKeuken.Web.Tests;

/// <summary>
/// Tests for the Tags management page: CRUD operations, filtering, and search.
/// </summary>
[TestFixture]
public class TagTests : PlaywrightTestBase
{
    /// <summary>
    /// Logs in using the pre-registered seed user from WebAppFixture.
    /// </summary>
    private async Task<Microsoft.Playwright.IPage> LoginAndNavigateToTagsAsync(Microsoft.Playwright.IBrowserContext context)
    {
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        // Navigate to Tags page via drawer
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Tags" }).ClickAsync();

        // Wait for the Tags table to be present (proves page + data loaded)
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        return page;
    }

    /// <summary>
    /// Creates a tag via the dialog and waits for it to appear in the table.
    /// </summary>
    private static async Task CreateTagAsync(Microsoft.Playwright.IPage page, string name, string type)
    {
        await page.Locator("button:has-text('Nieuwe tag')").ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        await dialog.GetByLabel("Naam").FillAsync(name);

        // Open the MudSelect dropdown (target the inner div with generated id)
        await dialog.Locator("div.mud-select[id]").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        // MudBlazor renders select options in a portal popover outside the dialog
        await page.Locator(".mud-popover-open .mud-list-item").GetByText(type, new() { Exact = true }).ClickAsync();
        await page.WaitForTimeoutAsync(300);

        await dialog.Locator("button:has-text('Aanmaken')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);
    }

    /// <summary>
    /// The Tags page should display the title and the "Nieuwe tag" button.
    /// </summary>
    [Test]
    public async Task TagsPage_DisplaysTitleAndCreateButton()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToTagsAsync(context);

        Assert.That(await page.Locator("h4").GetByText("Tags").IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("button:has-text('Nieuwe tag')").IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Creating a new tag via the dialog should add it to the table.
    /// </summary>
    [Test]
    public async Task CreateTag_AddsTagToTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToTagsAsync(context);

        var tagName = $"TestTag_{Guid.NewGuid():N}"[..16];

        await CreateTagAsync(page, tagName, "Ingrediënt");

        // Tag should appear in the table cell
        var tagCell = page.Locator("td[data-label='Naam']", new() { HasTextString = tagName });
        await tagCell.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await tagCell.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Editing a tag should update its name in the table.
    /// </summary>
    [Test]
    public async Task EditTag_UpdatesTagInTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToTagsAsync(context);

        var originalName = $"Edit_{Guid.NewGuid():N}"[..14];
        var updatedName = $"Upd_{Guid.NewGuid():N}"[..14];

        await CreateTagAsync(page, originalName, "Recept");

        // Click the edit button on the row containing our tag
        var row = page.Locator("tr", new() { HasText = originalName });
        await row.Locator("button").Nth(0).ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        var nameField = dialog.GetByLabel("Naam");
        await nameField.ClearAsync();
        await nameField.FillAsync(updatedName);

        await dialog.Locator("button:has-text('Opslaan')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        // Updated name should appear in the table cell
        var updatedCell = page.Locator("td[data-label='Naam']", new() { HasTextString = updatedName });
        Assert.That(await updatedCell.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Deleting a tag should remove it from the table.
    /// </summary>
    [Test]
    public async Task DeleteTag_RemovesTagFromTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToTagsAsync(context);

        var tagName = $"Del_{Guid.NewGuid():N}"[..14];

        await CreateTagAsync(page, tagName, "Maaltijd");

        var tagCell = page.Locator("td[data-label='Naam']", new() { HasTextString = tagName });
        await tagCell.WaitForAsync(new() { Timeout = 5000 });

        // Click the delete button (second icon button in the row)
        var row = page.Locator("tr", new() { HasText = tagName });
        await row.Locator("button").Nth(1).ClickAsync();

        // Confirm deletion in the dialog
        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });
        await dialog.Locator("button:has-text('Verwijderen')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        // Tag should no longer be visible in the table
        Assert.That(await tagCell.IsVisibleAsync(), Is.False);
    }

    /// <summary>
    /// Searching by name should filter the table results.
    /// </summary>
    [Test]
    public async Task SearchByName_FiltersTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToTagsAsync(context);

        var uniquePrefix = Guid.NewGuid().ToString("N")[..6];
        var matchName = $"{uniquePrefix}_Match";
        var otherName = $"Other_{Guid.NewGuid():N}"[..14];

        await CreateTagAsync(page, matchName, "Ingrediënt");
        await CreateTagAsync(page, otherName, "Recept");

        // Both should be visible initially
        var matchCell = page.Locator("td[data-label='Naam']", new() { HasTextString = matchName });
        Assert.That(await matchCell.IsVisibleAsync(), Is.True);

        // Type the unique prefix in the search box
        await page.GetByPlaceholder("Zoeken op naam...").FillAsync(uniquePrefix);
        await page.WaitForTimeoutAsync(500);

        // The matching tag should be visible, the other should not
        Assert.That(await matchCell.IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("tr", new() { HasText = otherName }).IsVisibleAsync(), Is.False);
    }

    /// <summary>
    /// The create dialog should show a live preview chip with the tag name.
    /// </summary>
    [Test]
    public async Task CreateDialog_ShowsPreviewChip()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToTagsAsync(context);

        await page.Locator("button:has-text('Nieuwe tag')").ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        await dialog.GetByLabel("Naam").FillAsync("Voorbeeldtag");
        await page.WaitForTimeoutAsync(300);

        // Preview chip should show the name
        var chip = dialog.Locator(".mud-chip").GetByText("Voorbeeldtag");
        Assert.That(await chip.IsVisibleAsync(), Is.True);

        // Cancel without creating
        await dialog.Locator("button:has-text('Annuleren')").ClickAsync();
    }
}
