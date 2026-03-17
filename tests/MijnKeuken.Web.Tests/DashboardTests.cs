

namespace MijnKeuken.Web.Tests;

/// <summary>
/// Tests for the Dashboard page: welcome message, empty state,
/// top recipes/ingredients stats after scheduling, and next delivery display.
/// </summary>
[TestFixture]
public class DashboardTests : PlaywrightTestBase
{
    private async Task<Microsoft.Playwright.IPage> LoginAndNavigateToDashboardAsync(
        Microsoft.Playwright.IBrowserContext context)
    {
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        return page;
    }

    private async Task NavigateToDashboardAsync(Microsoft.Playwright.IPage page)
    {
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Dashboard" }).ClickAsync();
        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 10000 });
    }

    private async Task NavigateToMenuAsync(Microsoft.Playwright.IPage page)
    {
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.Locator(".mud-drawer").GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Menu" }).ClickAsync();
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });
    }

    private async Task<string> CreateRecipeAsync(Microsoft.Playwright.IPage page)
    {
        var title = $"DashRec_{Guid.NewGuid():N}"[..16];

        await page.GotoAsync($"{BaseUrl}/recipes");
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });
        await page.GetByLabel("Titel").FillAsync(title);
        await page.Locator("button:has-text('Aanmaken')").ClickAsync();
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        return title;
    }

    /// <summary>
    /// Assigns a recipe to a specific row in the menu via the picker page.
    /// </summary>
    private async Task AssignRecipeToMenuRowAsync(
        Microsoft.Playwright.IPage page, int rowIndex, string recipeTitle)
    {
        await NavigateToMenuAsync(page);

        var targetRow = page.Locator(".mud-table tbody tr").Nth(rowIndex);
        var actionsCell = targetRow.Locator("td").Last;
        await actionsCell.Locator("button").First.ClickAsync();

        await page.GetByText("Recept kiezen voor").WaitForAsync(new() { Timeout = 10000 });

        var recipeCard = page.Locator(".recipe-card", new() { HasTextString = recipeTitle });
        await recipeCard.WaitForAsync(new() { Timeout = 5000 });
        await recipeCard.ClickAsync();

        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });
        await page.WaitForTimeoutAsync(1000);
    }

    /// <summary>
    /// Toggles delivery on for a specific menu row via the dialog.
    /// </summary>
    private async Task SetDeliveryOnMenuRowAsync(
        Microsoft.Playwright.IPage page, int rowIndex, string note)
    {
        await NavigateToMenuAsync(page);

        var targetRow = page.Locator(".mud-table tbody tr").Nth(rowIndex);
        var actionsCell = targetRow.Locator("td").Last;
        var buttons = actionsCell.Locator("button");
        var buttonCount = await buttons.CountAsync();
        await buttons.Nth(buttonCount - 2).ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });
        await dialog.GetByLabel("Bezorgnotitie").FillAsync(note);
        await dialog.Locator("button:has-text('Opslaan')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);
    }

    [Test]
    public async Task Dashboard_ShowsWelcomeMessage()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToDashboardAsync(context);

        Assert.That(await page.GetByText("Welkom bij MijnKeuken").IsVisibleAsync(), Is.True);
        Assert.That(await page.GetByText("Uw complete oplossing voor een keuken op maat.").IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task Dashboard_ShowsTopRecipesHeader()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToDashboardAsync(context);

        var recipesHeader = page.GetByText("Top 5 recepten");
        await recipesHeader.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await recipesHeader.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task Dashboard_ShowsTopIngredientsHeader()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToDashboardAsync(context);

        var ingredientsHeader = page.GetByText("Top 5 ingrediënten");
        await ingredientsHeader.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await ingredientsHeader.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task Dashboard_EmptyState_ShowsEmptyMessages()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToDashboardAsync(context);

        await page.GetByText("Top 5 recepten").WaitForAsync(new() { Timeout = 10000 });
        await page.GetByText("Top 5 ingrediënten").WaitForAsync(new() { Timeout = 10000 });

        // Both sections should be visible (either with data or empty state)
        Assert.That(await page.GetByText("Top 5 recepten").IsVisibleAsync(), Is.True);
        Assert.That(await page.GetByText("Top 5 ingrediënten").IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task Dashboard_AfterSchedulingRecipe_ShowsInTopRecipes()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToDashboardAsync(context);

        var recipeTitle = await CreateRecipeAsync(page);

        // Schedule the recipe on the 10th row (avoid conflicts with menu tests)
        await AssignRecipeToMenuRowAsync(page, 9, recipeTitle);

        await NavigateToDashboardAsync(page);

        // The recipe should appear in the top 5 section
        await page.GetByText("Top 5 recepten").WaitForAsync(new() { Timeout = 10000 });

        // Scope to the MudGrid to avoid matching snackbar messages
        var statsGrid = page.Locator(".mud-grid");
        var recipeInStats = statsGrid.GetByText(recipeTitle);
        await recipeInStats.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await recipeInStats.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task Dashboard_AfterSettingDelivery_ShowsNextDelivery()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToDashboardAsync(context);

        var deliveryNote = "TestBezorging";

        // Set delivery on the 11th row (avoid conflicts)
        await SetDeliveryOnMenuRowAsync(page, 10, deliveryNote);

        await NavigateToDashboardAsync(page);

        // The next delivery section should appear
        var deliveryHeader = page.GetByText("Volgende bezorging");
        await deliveryHeader.WaitForAsync(new() { Timeout = 10000 });
        Assert.That(await deliveryHeader.IsVisibleAsync(), Is.True);

        // The delivery note should be shown
        var noteText = page.GetByText(deliveryNote);
        Assert.That(await noteText.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task Dashboard_ScheduleCount_ShowsCorrectValue()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToDashboardAsync(context);

        var recipeTitle = await CreateRecipeAsync(page);

        // Schedule the same recipe on two different days (rows 11 and 12)
        await AssignRecipeToMenuRowAsync(page, 11, recipeTitle);
        await AssignRecipeToMenuRowAsync(page, 12, recipeTitle);

        await NavigateToDashboardAsync(page);

        await page.GetByText("Top 5 recepten").WaitForAsync(new() { Timeout = 10000 });

        // Scope to the MudGrid to avoid matching snackbar
        var statsGrid = page.Locator(".mud-grid");
        await statsGrid.GetByText(recipeTitle).WaitForAsync(new() { Timeout = 5000 });

        // Find the list item containing this recipe and verify it has the "2×" chip
        var recipeItem = statsGrid.Locator(".mud-list-item", new() { HasTextString = recipeTitle });
        var countChip = recipeItem.Locator(".mud-chip", new() { HasTextString = "2×" });
        Assert.That(await countChip.IsVisibleAsync(), Is.True);
    }
}
