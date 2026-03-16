using MijnKeuken.Application.Recipes.DTOs;

namespace MijnKeuken.Web.Services;

/// <summary>
/// Scoped state holder for passing scraped recipe data from the list page to the create form.
/// </summary>
public class ScrapedRecipeState
{
    public ScrapedRecipeDto? Data { get; set; }
    public string SourceUrl { get; set; } = string.Empty;

    public (ScrapedRecipeDto? Data, string SourceUrl) Consume()
    {
        var result = (Data, SourceUrl);
        Data = null;
        SourceUrl = string.Empty;
        return result;
    }
}
