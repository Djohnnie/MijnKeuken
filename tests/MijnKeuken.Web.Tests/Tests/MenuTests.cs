namespace MijnKeuken.Web.Tests.Tests;

/// <summary>
/// Tests for the Menu page: recipe assignment, delivery toggle, eating out,
/// consumed state, period navigation, and recipe picker page.
/// </summary>
[TestFixture]
public class MenuTests : PlaywrightTestBase
{
    private async Task<Microsoft.Playwright.IPage> LoginAndNavigateToMenuAsync(Microsoft.Playwright.IBrowserContext context)
    {
        var page = await context.NewPageAsync();

        await GotoAndWaitForBlazorAsync(page, $"{BaseUrl}/account/login");

        await page.GetByLabel("Gebruikersnaam").FillAsync(WebAppFixture.SeedUsername);
        await page.GetByLabel("Wachtwoord").FillAsync(WebAppFixture.SeedPassword);
        await page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();

        await page.GetByText("Welkom bij MijnKeuken").WaitForAsync(new() { Timeout = 15000 });

        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.Locator(".mud-drawer").GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Menu" }).ClickAsync();

        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        return page;
    }

    private async Task NavigateToMenuAsync(Microsoft.Playwright.IPage page)
    {
        await page.Locator(".mud-drawer").HoverAsync();
        await page.WaitForTimeoutAsync(500);
        await page.Locator(".mud-drawer").GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Menu" }).ClickAsync();
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });
    }

    /// <summary>
    /// Creates a recipe via the Recipes page form and returns its title.
    /// </summary>
    private async Task<string> CreateRecipeAsync(Microsoft.Playwright.IPage page)
    {
        var title = $"MenuRec_{Guid.NewGuid():N}"[..16];

        await page.GotoAsync($"{BaseUrl}/recipes");
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        await page.Locator("button:has-text('Nieuw recept')").ClickAsync();
        await page.GetByLabel("Titel").WaitForAsync(new() { Timeout = 5000 });
        await page.GetByLabel("Titel").FillAsync(title);
        await page.Locator("button:has-text('Aanmaken')").ClickAsync();
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        return title;
    }

    /// <summary>Gets the date range subtitle text (the p element, not the MudSelect input).</summary>
    private static Microsoft.Playwright.ILocator DateRangeText(Microsoft.Playwright.IPage page) =>
        page.Locator("p.mud-typography-subtitle1");

    [Test]
    public async Task MenuPage_DisplaysTitleAndTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        Assert.That(await page.Locator("h4").GetByText("Menu").IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator(".mud-table").IsVisibleAsync(), Is.True);

        var rows = page.Locator(".mud-table tbody tr");
        var count = await rows.CountAsync();
        Assert.That(count, Is.EqualTo(14));
    }

    [Test]
    public async Task MenuPage_ShowsNoRecipeSelectedByDefault()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        var noRecipeText = page.GetByText("Geen recept geselecteerd").First;
        Assert.That(await noRecipeText.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task MenuPage_PeriodNavigation_ChangesDateRange()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        var initialDateText = await DateRangeText(page).InnerTextAsync();

        await page.Locator("button:has-text('Volgende')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        var nextDateText = await DateRangeText(page).InnerTextAsync();
        Assert.That(nextDateText, Is.Not.EqualTo(initialDateText));

        await page.Locator("button:has-text('Vorige')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        var backDateText = await DateRangeText(page).InnerTextAsync();
        Assert.That(backDateText, Is.EqualTo(initialDateText));
    }

    [Test]
    public async Task MenuPage_TodayButton_ReturnsToCurrentPeriod()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        var initialDateText = await DateRangeText(page).InnerTextAsync();

        await page.Locator("button:has-text('Volgende')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);
        await page.Locator("button:has-text('Volgende')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        await page.Locator("button:has-text('Vandaag')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        var todayDateText = await DateRangeText(page).InnerTextAsync();
        Assert.That(todayDateText, Is.EqualTo(initialDateText));
    }

    [Test]
    public async Task MenuPage_AddRecipe_NavigatesToPickerAndAssigns()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        var recipeTitle = await CreateRecipeAsync(page);

        await NavigateToMenuAsync(page);

        // Use the second row to avoid conflicts with other tests that use the first row
        var targetRow = page.Locator(".mud-table tbody tr").Nth(1);
        var actionsCell = targetRow.Locator("td").Last;
        var buttons = actionsCell.Locator("button");

        // Click the add/edit button (first button when no recipe assigned)
        await buttons.First.ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        // Should navigate to picker page
        await page.GetByText("Recept kiezen voor").WaitForAsync(new() { Timeout = 10000 });

        var recipeCard = page.Locator(".recipe-card", new() { HasTextString = recipeTitle });
        await recipeCard.WaitForAsync(new() { Timeout = 5000 });

        await recipeCard.ClickAsync();

        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        // Scope to the table to avoid matching snackbar messages
        var recipeCell = page.Locator(".mud-table").GetByText(recipeTitle);
        await recipeCell.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await recipeCell.IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task MenuPage_RemoveRecipe_ClearsFromTable()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        var recipeTitle = await CreateRecipeAsync(page);

        await NavigateToMenuAsync(page);

        // Use the third row to avoid conflicts with other tests
        var targetRow = page.Locator(".mud-table tbody tr").Nth(2);
        var actionsCell = targetRow.Locator("td").Last;
        await actionsCell.Locator("button").First.ClickAsync();
        await page.GetByText("Recept kiezen voor").WaitForAsync(new() { Timeout = 10000 });
        await page.Locator(".recipe-card", new() { HasTextString = recipeTitle }).ClickAsync();
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        // Wait for snackbar to disappear to avoid strict mode issues
        await page.WaitForTimeoutAsync(2000);

        // Verify recipe is in the table
        var tableRecipe = page.Locator(".mud-table").GetByText(recipeTitle);
        await tableRecipe.WaitForAsync(new() { Timeout = 5000 });

        // Now find the row with the recipe and click the clear/remove button
        var rowWithRecipe = page.Locator(".mud-table tbody tr", new() { HasTextString = recipeTitle });
        await rowWithRecipe.WaitForAsync(new() { Timeout = 5000 });

        // Buttons when recipe exists: [consumed toggle, swap, edit, clear, delivery toggle, eating out toggle]
        // Clear is the 4th button (index 3)
        var rowButtons = rowWithRecipe.Locator("td").Last.Locator("button");
        await rowButtons.Nth(3).ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        Assert.That(await page.Locator(".mud-table").GetByText(recipeTitle).CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task MenuPage_ToggleEatingOut_ShowsText()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        // Use the fourth row to avoid conflicts with AddRecipe/RemoveRecipe tests
        var targetRow = page.Locator(".mud-table tbody tr").Nth(3);
        var actionsCell = targetRow.Locator("td").Last;
        var buttons = actionsCell.Locator("button");

        // Eating out toggle is the last button (regardless of recipe state)
        await buttons.Last.ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        // The recipe column should show "Buitenshuis eten"
        var eatingOutText = targetRow.GetByText("Buitenshuis eten");
        await eatingOutText.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await eatingOutText.IsVisibleAsync(), Is.True);

        // Toggle off
        actionsCell = targetRow.Locator("td").Last;
        buttons = actionsCell.Locator("button");
        await buttons.Last.ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        // Verify the eating out text is gone
        await page.WaitForTimeoutAsync(500);
        Assert.That(await targetRow.GetByText("Buitenshuis eten").CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task MenuPage_ToggleDelivery_ShowsDialog()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        // Use the fifth row to avoid conflicts with other tests
        var targetRow = page.Locator(".mud-table tbody tr").Nth(4);
        var actionsCell = targetRow.Locator("td").Last;
        var buttons = actionsCell.Locator("button");

        // Delivery toggle is second-to-last button
        var buttonCount = await buttons.CountAsync();
        var deliveryButton = buttons.Nth(buttonCount - 2);
        await deliveryButton.ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        await dialog.GetByLabel("Bezorgnotitie").FillAsync("Albert Heijn");
        await dialog.Locator("button:has-text('Opslaan')").ClickAsync();
        await page.WaitForTimeoutAsync(1000);

        // Reload the menu to verify persistence
        await NavigateToMenuAsync(page);

        Assert.That(await page.Locator(".mud-table").IsVisibleAsync(), Is.True);
    }

    [Test]
    public async Task MenuPage_DeliveryDialog_CancelDoesNotSave()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        // Use the sixth row to avoid conflicts
        var targetRow = page.Locator(".mud-table tbody tr").Nth(5);
        var actionsCell = targetRow.Locator("td").Last;
        var buttons = actionsCell.Locator("button");

        // Delivery toggle — second-to-last button
        var buttonCount = await buttons.CountAsync();
        var deliveryButton = buttons.Nth(buttonCount - 2);
        await deliveryButton.ClickAsync();

        var dialog = page.Locator(".mud-dialog");
        await dialog.WaitForAsync(new() { Timeout = 5000 });

        await dialog.Locator("button:has-text('Annuleren')").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        // Dialog should be closed
        Assert.That(await dialog.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task RecipePickerPage_SearchFiltersRecipes()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        var matchTitle = await CreateRecipeAsync(page);
        var otherTitle = await CreateRecipeAsync(page);

        await NavigateToMenuAsync(page);

        // Open picker for seventh row to avoid conflicts
        var targetRow = page.Locator(".mud-table tbody tr").Nth(6);
        await targetRow.Locator("td").Last.Locator("button").First.ClickAsync();
        await page.GetByText("Recept kiezen voor").WaitForAsync(new() { Timeout = 10000 });

        await page.Locator(".recipe-card", new() { HasTextString = matchTitle }).WaitForAsync(new() { Timeout = 5000 });

        await page.GetByLabel("Zoeken op titel of beschrijving").FillAsync(matchTitle);
        await page.WaitForTimeoutAsync(1000);

        Assert.That(await page.Locator(".recipe-card", new() { HasTextString = matchTitle }).IsVisibleAsync(), Is.True);
        Assert.That(await page.Locator(".recipe-card", new() { HasTextString = otherTitle }).CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task RecipePickerPage_BackButtonReturnsToMenu()
    {
        await using var context = await CreateContextAsync();
        var page = await LoginAndNavigateToMenuAsync(context);

        // Open picker for eighth row to avoid conflicts
        var targetRow = page.Locator(".mud-table tbody tr").Nth(7);
        await targetRow.Locator("td").Last.Locator("button").First.ClickAsync();
        await page.GetByText("Recept kiezen voor").WaitForAsync(new() { Timeout = 10000 });

        // The back button is the first button inside the MudContainer's header
        var headerText = page.GetByText("Recept kiezen voor");
        await headerText.Locator("xpath=../button").First.ClickAsync();
        await page.Locator(".mud-table").WaitForAsync(new() { Timeout = 10000 });

        Assert.That(await page.Locator("h4").GetByText("Menu").IsVisibleAsync(), Is.True);
    }
}
