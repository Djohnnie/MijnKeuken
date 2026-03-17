namespace MijnKeuken.Domain.Entities;

public class Recipe : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public int Servings { get; set; } = 2;
    public string SourceUrl { get; set; } = string.Empty;
    public bool IsArchived { get; set; }

    public List<RecipeTag> RecipeTags { get; set; } = [];
    public List<RecipeIngredient> RecipeIngredients { get; set; } = [];
}
