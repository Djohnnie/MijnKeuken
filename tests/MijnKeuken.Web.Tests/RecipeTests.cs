using MijnKeuken.Web.Tests.Helpers;

namespace MijnKeuken.Web.Tests;

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

        // Wait for navigation back to the recipes list — use a list-page-specific locator
        // because .mud-table now also matches the DataGrid on the recipe form
        await page.Locator("button:has-text('Nieuw recept')").WaitForAsync(new() { Timeout = 10000 });
    }

    private static async Task AddFreeTextIngredientInGridAsync(Microsoft.Playwright.IPage page, string name)
    {
        var ingredientInput = page.GetByPlaceholder("Nieuw ingrediënt...");
        await ingredientInput.FillAsync(name);
        await page.WaitForTimeoutAsync(300);

        var newRow = page.Locator(".ingredient-grid tr",
            new() { Has = page.GetByPlaceholder("Nieuw ingrediënt...") });
        await newRow.Locator("td").Last.Locator("button").ClickAsync();
        await page.WaitForTimeoutAsync(500);
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
        await page.Locator("button:has-text('Nieuw recept')").WaitForAsync(new() { Timeout = 10000 });

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
    public async Task RecipeForm_IngredientGrid_DisplaysColumnsAndNewRow()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        var grid = page.Locator(".ingredient-grid");
        await grid.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await grid.IsVisibleAsync(), Is.True);

        Assert.That(await grid.GetByText("Ingrediënt").IsVisibleAsync(), Is.True);
        Assert.That(await grid.GetByText("Hoeveelheid").IsVisibleAsync(), Is.True);
        Assert.That(await grid.GetByText("Eenheid").IsVisibleAsync(), Is.True);
        Assert.That(await page.GetByPlaceholder("Nieuw ingrediënt...").IsVisibleAsync(), Is.True);

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task RecipeForm_AddFreeTextIngredient_ShowsAsReadOnly()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        var name = $"Ing_{Guid.NewGuid():N}"[..10];
        await AddFreeTextIngredientInGridAsync(page, name);

        var grid = page.Locator(".ingredient-grid");
        var ingredientRow = grid.Locator("tr", new() { HasTextString = name });
        await ingredientRow.WaitForAsync(new() { Timeout = 3000 });

        // Name should be in a <p> element (MudText), not an editable input
        Assert.That(await ingredientRow.Locator("p", new() { HasTextString = name }).IsVisibleAsync(), Is.True);

        // The new row's input should be cleared
        Assert.That(await page.GetByPlaceholder("Nieuw ingrediënt...").InputValueAsync(), Is.EqualTo(""));

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task RecipeForm_AddIngredient_ViaEnterKey()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        var name = $"Enter_{Guid.NewGuid():N}"[..12];
        var ingredientInput = page.GetByPlaceholder("Nieuw ingrediënt...");
        await ingredientInput.FillAsync(name);
        await page.WaitForTimeoutAsync(300);
        await ingredientInput.PressAsync("Enter");
        await page.WaitForTimeoutAsync(500);

        var grid = page.Locator(".ingredient-grid");
        Assert.That(await grid.Locator("p", new() { HasTextString = name }).IsVisibleAsync(), Is.True);

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task RecipeForm_FreeTextIngredient_ShowsFreeTextIcon()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        var name = $"Icon_{Guid.NewGuid():N}"[..10];
        await AddFreeTextIngredientInGridAsync(page, name);

        var grid = page.Locator(".ingredient-grid");
        var ingredientRow = grid.Locator("tr", new() { HasTextString = name });
        await ingredientRow.WaitForAsync(new() { Timeout = 3000 });

        // Free-text ingredients should show the "Vrije tekst" SVG title
        Assert.That(await ingredientRow.Locator("svg title").TextContentAsync(), Is.EqualTo("Vrije tekst"));

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task RecipeForm_DeleteIngredient_RemovesFromGrid()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        var name = $"Del_{Guid.NewGuid():N}"[..10];
        await AddFreeTextIngredientInGridAsync(page, name);

        var grid = page.Locator(".ingredient-grid");
        var ingredientRow = grid.Locator("tr", new() { HasTextString = name });
        await ingredientRow.WaitForAsync(new() { Timeout = 3000 });

        // Click the delete button on the ingredient row
        await ingredientRow.Locator("td").Last.Locator("button").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.That(await grid.Locator("p", new() { HasTextString = name }).IsVisibleAsync(), Is.False);

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task RecipeForm_DuplicateIngredient_NotAdded()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        var name = $"Dup_{Guid.NewGuid():N}"[..10];
        await AddFreeTextIngredientInGridAsync(page, name);
        await AddFreeTextIngredientInGridAsync(page, name);

        var grid = page.Locator(".ingredient-grid");
        Assert.That(await grid.Locator("p", new() { HasTextString = name }).CountAsync(), Is.EqualTo(1));

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task CreateRecipeWithIngredient_PersistsOnEdit()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        var title = $"IngRec_{Guid.NewGuid():N}"[..14];
        var ingredientName = $"Ing_{Guid.NewGuid():N}"[..10];

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        await page.GetByLabel("Titel").FillAsync(title);
        await AddFreeTextIngredientInGridAsync(page, ingredientName);

        await page.Locator("button:has-text('Aanmaken')").ClickAsync();
        await page.Locator("button:has-text('Nieuw recept')").WaitForAsync(new() { Timeout = 10000 });

        // Click edit on the created recipe
        var row = page.Locator("tr", new() { HasText = title });
        await row.Locator("button").Nth(0).ClickAsync();

        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });
        await page.Locator(".ingredient-grid").WaitForAsync(new() { Timeout = 5000 });

        // The ingredient should be persisted and shown as read-only text
        var grid = page.Locator(".ingredient-grid");
        Assert.That(await grid.Locator("p", new() { HasTextString = ingredientName }).IsVisibleAsync(), Is.True);

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
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

    [Test]
    public async Task RecipeForm_ServingsField_VisibleWithDefaultValue()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        var servingsField = page.GetByLabel("Aantal personen");
        Assert.That(await servingsField.IsVisibleAsync(), Is.True);
        Assert.That(await servingsField.InputValueAsync(), Is.EqualTo("2"));

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task RecipeForm_ServingsField_EditableOnCreate()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        var servingsField = page.GetByLabel("Aantal personen");
        Assert.That(await servingsField.GetAttributeAsync("readonly"), Is.Null);

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task RecipeForm_Servings_PersistsOnCreateAndReadOnlyOnEdit()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        var title = $"Srv_{Guid.NewGuid():N}"[..14];

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        await page.GetByLabel("Titel").FillAsync(title);
        var servingsField = page.GetByLabel("Aantal personen");
        await servingsField.ClearAsync();
        await servingsField.FillAsync("6");

        await page.Locator("button:has-text('Aanmaken')").ClickAsync();
        await page.Locator("button:has-text('Nieuw recept')").WaitForAsync(new() { Timeout = 10000 });

        // Edit the recipe
        var row = page.Locator("tr", new() { HasText = title });
        await row.Locator("button").Nth(0).ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        // Servings should show the saved value and be read-only
        var editServings = page.GetByLabel("Aantal personen");
        Assert.That(await editServings.InputValueAsync(), Is.EqualTo("6"));
        Assert.That(await editServings.GetAttributeAsync("readonly"), Is.Not.Null);

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task RecipeForm_SourceUrl_HiddenOnManualCreate()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        // Source URL field should not be visible when creating manually
        Assert.That(await page.GetByLabel("Bron URL").IsVisibleAsync(), Is.False);

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }

    [Test]
    public async Task RecipeForm_TitleAndServings_OnSameLine()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToRecipesAsync(context);

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });

        // Both fields should be visible and on the same line (within same flex container)
        var titleField = page.GetByLabel("Titel");
        var servingsField = page.GetByLabel("Aantal personen");
        Assert.That(await titleField.IsVisibleAsync(), Is.True);
        Assert.That(await servingsField.IsVisibleAsync(), Is.True);

        // Verify they share the same vertical position (same line)
        var titleBox = await titleField.BoundingBoxAsync();
        var servingsBox = await servingsField.BoundingBoxAsync();
        Assert.That(titleBox, Is.Not.Null);
        Assert.That(servingsBox, Is.Not.Null);
        Assert.That(Math.Abs(titleBox!.Y - servingsBox!.Y), Is.LessThan(10),
            "Title and servings should be on the same line");

        await page.Locator("button:has-text('Annuleren')").ClickAsync();
    }
}
