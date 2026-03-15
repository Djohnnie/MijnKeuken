using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MijnKeuken.Application.Interfaces;
using MijnKeuken.Infrastructure.Persistence;
using MijnKeuken.Infrastructure.Services;

namespace MijnKeuken.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "CONNECTION_STRING environment variable is not set.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IStorageLocationRepository, StorageLocationRepository>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? "MijnKeuken-Dev-Secret-Key-Min-32-Characters!!";

        var jwtSettings = new JwtSettings { Secret = jwtSecret };
        services.AddSingleton(jwtSettings);
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
