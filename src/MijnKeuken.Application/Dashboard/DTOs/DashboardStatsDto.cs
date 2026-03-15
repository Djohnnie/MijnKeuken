namespace MijnKeuken.Application.Dashboard.DTOs;

public record DashboardStatsDto(
    List<RecipeUsageStatDto> TopRecipes,
    List<IngredientUsageStatDto> TopIngredients,
    NextDeliveryDto? NextDelivery);

public record RecipeUsageStatDto(Guid RecipeId, string RecipeTitle, int ScheduleCount);

public record IngredientUsageStatDto(Guid IngredientId, string IngredientName, int UsageCount);

public record NextDeliveryDto(DateOnly Date, string Note);
