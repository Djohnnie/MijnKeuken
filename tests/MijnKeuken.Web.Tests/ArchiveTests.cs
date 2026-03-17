namespace MijnKeuken.Web.Tests;

/// <summary>
/// Tests for archiving/unarchiving recipes and ingredients,
/// and the Archive page filtering and search.
/// </summary>
[TestFixture]
public class ArchiveTests : PlaywrightTestBase
{
    private async Task<Microsoft.Playwright.IPage> LoginAsync(Microsoft.Playwright.IBrowserContext context)
    {
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        return page;
    }

    private static async Task NavigateToRecipesAsync(Microsoft.Playwright.IPage page)
    {
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Recepten" }).ClickAsync();
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });
    }

    private static async Task NavigateToIngredientsAsync(Microsoft.Playwright.IPage page)
    {
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Ingrediënten" }).ClickAsync();
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });
    }

    private static async Task NavigateToArchiveAsync(Microsoft.Playwright.IPage page)
    {
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Archief" }).ClickAsync();
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });
    }

    private static async Task CreateRecipeAsync(Microsoft.Playwright.IPage page, string title)
    {
        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });
        await page.GetByLabel("Titel").FillAsync(title);
        await page.Locator("button:has-text('Aanmaken')").ClickAsync();
        await page.Locator("button:has-text('Nieuw recept')").WaitForAsync(new() { Timeout = 10000 });
    }

    private static async Task CreateIngredientAsync(Microsoft.Playwright.IPage page, string title)
    {
        await page.Locator("button:has-text('Nieuw ingrediënt')").ClickAsync();
        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });
        await dialog.GetByLabel("Titel").FillAsync(title);
        await dialog.Locator("button:has-text('Aanmaken')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);
    }

    private static async Task ArchiveItemInRow(Microsoft.Playwright.IPage page, string title, string confirmDialogTitle)
    {
        var row = page.Locator("tr", new() { HasText = title });
        // Archive is the second action button (index 1: Edit, Archive, Delete)
        await row.Locator("button").Nth(1).ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });
        await dialog.Locator("button:has-text('Archiveren')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);
    }

    // ── Archive recipe ──────────────────────────────────────────────

    [Test]
    public async Task ArchiveRecipe_RemovesFromRecipesTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);
        await NavigateToRecipesAsync(page);

        var title = $"ArcR_{Guid.NewGuid():N}"[..14];
        await CreateRecipeAsync(page, title);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 5000 });

        await ArchiveItemInRow(page, title, "Recept archiveren");

        Assert.That(await titleCell.IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task ArchiveRecipe_AppearsInArchivePage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);
        await NavigateToRecipesAsync(page);

        var title = $"ArcR_{Guid.NewGuid():N}"[..14];
        await CreateRecipeAsync(page, title);
        await ArchiveItemInRow(page, title, "Recept archiveren");

        await NavigateToArchiveAsync(page);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await titleCell.IsVisibleAsync(), Is.True);
    }

    // ── Archive ingredient ──────────────────────────────────────────

    [Test]
    public async Task ArchiveIngredient_RemovesFromIngredientsTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);
        await NavigateToIngredientsAsync(page);

        var title = $"ArcI_{Guid.NewGuid():N}"[..14];
        await CreateIngredientAsync(page, title);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 5000 });

        await ArchiveItemInRow(page, title, "Ingrediënt archiveren");

        Assert.That(await titleCell.IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task ArchiveIngredient_AppearsInArchivePage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);
        await NavigateToIngredientsAsync(page);

        var title = $"ArcI_{Guid.NewGuid():N}"[..14];
        await CreateIngredientAsync(page, title);
        await ArchiveItemInRow(page, title, "Ingrediënt archiveren");

        await NavigateToArchiveAsync(page);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await titleCell.IsVisibleAsync(), Is.True);
    }

    /// <summary>
    /// Navigates to the archive page via direct URL (full page load) and
    /// searches for a title to filter the table. Uses keyboard input
    /// (PressSequentially) to reliably trigger MudBlazor's Immediate binding.
    /// </summary>
    private async Task NavigateToArchiveAndSearchAsync(Microsoft.Playwright.IPage page, string title)
    {
        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/archive");
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        var searchBox = page.GetByPlaceholder("Zoeken op titel...");
        await searchBox.ClickAsync();
        await searchBox.PressSequentiallyAsync(title, new() { Delay = 50 });
        await page.WaitForTimeoutAsync(1000);
    }

    private static async Task UnarchiveItemAsync(Microsoft.Playwright.IPage page, string title)
    {
        var row = page.Locator("tr", new() { HasText = title });
        await row.Locator("button").First.ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });
        await dialog.Locator("button:has-text('Herstellen')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);
    }

    // ── Unarchive recipe ────────────────────────────────────────────

    [Test]
    public async Task UnarchiveRecipe_RemovesFromArchivePage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);
        await NavigateToRecipesAsync(page);

        var title = $"UnR_{Guid.NewGuid():N}"[..14];
        await CreateRecipeAsync(page, title);
        await ArchiveItemInRow(page, title, "Recept archiveren");

        await NavigateToArchiveAndSearchAsync(page, title);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 10000 });

        await UnarchiveItemAsync(page, title);

        Assert.That(await titleCell.IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task UnarchiveRecipe_ReappearsInRecipesPage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);
        await NavigateToRecipesAsync(page);

        var title = $"UnR_{Guid.NewGuid():N}"[..14];
        await CreateRecipeAsync(page, title);
        await ArchiveItemInRow(page, title, "Recept archiveren");

        await NavigateToArchiveAndSearchAsync(page, title);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 10000 });

        await UnarchiveItemAsync(page, title);

        await NavigateToRecipesAsync(page);

        var recipeTitleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await recipeTitleCell.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await recipeTitleCell.IsVisibleAsync(), Is.True);
    }

    // ── Unarchive ingredient ────────────────────────────────────────

    [Test]
    public async Task UnarchiveIngredient_RemovesFromArchivePage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);
        await NavigateToIngredientsAsync(page);

        var title = $"UnI_{Guid.NewGuid():N}"[..14];
        await CreateIngredientAsync(page, title);
        await ArchiveItemInRow(page, title, "Ingrediënt archiveren");

        await NavigateToArchiveAndSearchAsync(page, title);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 10000 });

        await UnarchiveItemAsync(page, title);

        Assert.That(await titleCell.IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task UnarchiveIngredient_ReappearsInIngredientsPage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);
        await NavigateToIngredientsAsync(page);

        var title = $"UnI_{Guid.NewGuid():N}"[..14];
        await CreateIngredientAsync(page, title);
        await ArchiveItemInRow(page, title, "Ingrediënt archiveren");

        await NavigateToArchiveAndSearchAsync(page, title);

        var titleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await titleCell.WaitForAsync(new() { Timeout = 10000 });

        await UnarchiveItemAsync(page, title);

        await NavigateToIngredientsAsync(page);

        var ingredientTitleCell = page.Locator("td[data-label='Titel']", new() { HasTextString = title });
        await ingredientTitleCell.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await ingredientTitleCell.IsVisibleAsync(), Is.True);
    }

    // ── Archive page: filter by type ────────────────────────────────

    [Test]
    public async Task ArchivePage_FilterByRecipes_ShowsOnlyRecipes()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);

        // Create and archive a recipe
        await NavigateToRecipesAsync(page);
        var recipeTitle = $"FltR_{Guid.NewGuid():N}"[..14];
        await CreateRecipeAsync(page, recipeTitle);
        await ArchiveItemInRow(page, recipeTitle, "Recept archiveren");

        // Create and archive an ingredient
        await NavigateToIngredientsAsync(page);
        var ingredientTitle = $"FltI_{Guid.NewGuid():N}"[..14];
        await CreateIngredientAsync(page, ingredientTitle);
        await ArchiveItemInRow(page, ingredientTitle, "Ingrediënt archiveren");

        await NavigateToArchiveAsync(page);

        // Both should be visible initially
        await page.Locator("td[data-label='Titel']", new() { HasTextString = recipeTitle })
            .WaitForAsync(new() { Timeout = 5000 });

        // Click the "Recepten" filter chip
        await page.Locator(".mud-chip", new() { HasTextString = "Recepten" }).ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.That(await page.Locator("td[data-label='Titel']", new() { HasTextString = recipeTitle }).IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("td[data-label='Titel']", new() { HasTextString = ingredientTitle }).IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task ArchivePage_FilterByIngredients_ShowsOnlyIngredients()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);

        // Create and archive a recipe
        await NavigateToRecipesAsync(page);
        var recipeTitle = $"FltR_{Guid.NewGuid():N}"[..14];
        await CreateRecipeAsync(page, recipeTitle);
        await ArchiveItemInRow(page, recipeTitle, "Recept archiveren");

        // Create and archive an ingredient
        await NavigateToIngredientsAsync(page);
        var ingredientTitle = $"FltI_{Guid.NewGuid():N}"[..14];
        await CreateIngredientAsync(page, ingredientTitle);
        await ArchiveItemInRow(page, ingredientTitle, "Ingrediënt archiveren");

        await NavigateToArchiveAsync(page);

        await page.Locator("td[data-label='Titel']", new() { HasTextString = ingredientTitle })
            .WaitForAsync(new() { Timeout = 5000 });

        // Click the "Ingrediënten" filter chip
        await page.Locator(".mud-chip", new() { HasTextString = "Ingrediënten" }).ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.That(await page.Locator("td[data-label='Titel']", new() { HasTextString = ingredientTitle }).IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("td[data-label='Titel']", new() { HasTextString = recipeTitle }).IsVisibleAsync(), Is.False);
    }

    [Test]
    public async Task ArchivePage_FilterAll_ShowsBothTypes()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);

        await NavigateToRecipesAsync(page);
        var recipeTitle = $"AllR_{Guid.NewGuid():N}"[..14];
        await CreateRecipeAsync(page, recipeTitle);
        await ArchiveItemInRow(page, recipeTitle, "Recept archiveren");

        await NavigateToIngredientsAsync(page);
        var ingredientTitle = $"AllI_{Guid.NewGuid():N}"[..14];
        await CreateIngredientAsync(page, ingredientTitle);
        await ArchiveItemInRow(page, ingredientTitle, "Ingrediënt archiveren");

        await NavigateToArchiveAsync(page);

        // Filter to recipes first
        await page.Locator(".mud-chip", new() { HasTextString = "Recepten" }).ClickAsync();
        await page.WaitForTimeoutAsync(500);

        // Then switch back to "Alles"
        await page.Locator(".mud-chip", new() { HasTextString = "Alles" }).ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.That(await page.Locator("td[data-label='Titel']", new() { HasTextString = recipeTitle }).IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("td[data-label='Titel']", new() { HasTextString = ingredientTitle }).IsVisibleAsync(), Is.True);
    }

    // ── Archive page: search ────────────────────────────────────────

    [Test]
    public async Task ArchivePage_SearchByTitle_FiltersTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);

        var uniquePrefix = Guid.NewGuid().ToString("N")[..6];

        await NavigateToRecipesAsync(page);
        var matchTitle = $"{uniquePrefix}_Match";
        await CreateRecipeAsync(page, matchTitle);
        await ArchiveItemInRow(page, matchTitle, "Recept archiveren");

        await NavigateToIngredientsAsync(page);
        var otherTitle = $"Other_{Guid.NewGuid():N}"[..14];
        await CreateIngredientAsync(page, otherTitle);
        await ArchiveItemInRow(page, otherTitle, "Ingrediënt archiveren");

        await NavigateToArchiveAsync(page);

        await page.Locator("td[data-label='Titel']", new() { HasTextString = matchTitle })
            .WaitForAsync(new() { Timeout = 5000 });

        await page.GetByPlaceholder("Zoeken op titel...").FillAsync(uniquePrefix);
        await page.WaitForTimeoutAsync(500);

        Assert.That(await page.Locator("td[data-label='Titel']", new() { HasTextString = matchTitle }).IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator("td[data-label='Titel']", new() { HasTextString = otherTitle }).IsVisibleAsync(), Is.False);
    }

    // ── Archive page: display ───────────────────────────────────────

    [Test]
    public async Task ArchivePage_DisplaysTitleAndTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);
        await NavigateToArchiveAsync(page);

        Assert.That(await page.Locator("h4").GetByText("Archief").IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator(".mud-table").IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task ArchivePage_ShowsTypeChips()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAsync(context);

        await NavigateToRecipesAsync(page);
        var title = $"Chip_{Guid.NewGuid():N}"[..14];
        await CreateRecipeAsync(page, title);
        await ArchiveItemInRow(page, title, "Recept archiveren");

        await NavigateToArchiveAsync(page);

        var row = page.Locator("tr", new() { HasText = title });
        await row.WaitForAsync(new() { Timeout = 5000 });

        // The type column should show a "Recept" chip
        var typeChip = row.Locator(".mud-chip", new() { HasTextString = "Recept" });
        Assert.That(await typeChip.IsVisibleAsync(), Is.True);
    }
}
