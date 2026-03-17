using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<IngredientTag> IngredientTags => Set<IngredientTag>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<MenuEntry> MenuEntries => Set<MenuEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureBaseEntity<User>(modelBuilder);
        ConfigureBaseEntity<Tag>(modelBuilder);
        ConfigureBaseEntity<StorageLocation>(modelBuilder);
        ConfigureBaseEntity<Ingredient>(modelBuilder);
        ConfigureBaseEntity<Recipe>(modelBuilder);
        ConfigureBaseEntity<MenuEntry>(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(t => new { t.Name, t.Type }).IsUnique();
            entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Color).HasMaxLength(9).IsRequired();
            entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<StorageLocation>(entity =>
        {
            entity.HasIndex(l => l.Name).IsUnique();
            entity.Property(l => l.Name).HasMaxLength(100).IsRequired();
            entity.Property(l => l.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasIndex(i => i.Title).IsUnique();
            entity.Property(i => i.Title).HasMaxLength(200).IsRequired();
            entity.Property(i => i.Description).HasMaxLength(1000);
            entity.Property(i => i.CustomUnitDescription).HasMaxLength(50);
            entity.Property(i => i.Barcode).HasMaxLength(50);
            entity.Property(i => i.StoreUrl).HasMaxLength(500);
            entity.Property(i => i.Unit).HasConversion<string>().HasMaxLength(20);
            entity.Property(i => i.AmountAvailable).HasPrecision(18, 4);
            entity.Property(i => i.AmountTotal).HasPrecision(18, 4);

            entity.HasOne(i => i.StorageLocation)
                .WithMany()
                .HasForeignKey(i => i.StorageLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<IngredientTag>(entity =>
        {
            entity.HasKey(it => new { it.IngredientId, it.TagId });

            entity.HasOne(it => it.Ingredient)
                .WithMany(i => i.IngredientTags)
                .HasForeignKey(it => it.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(it => it.Tag)
                .WithMany()
                .HasForeignKey(it => it.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasIndex(r => r.Title).IsUnique();
            entity.Property(r => r.Title).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Description).HasMaxLength(1000);
            entity.Property(r => r.Plan).HasMaxLength(10000);
            entity.Property(r => r.Servings).HasDefaultValue(2);
            entity.Property(r => r.SourceUrl).HasMaxLength(2000).HasDefaultValue(string.Empty);
        });

        modelBuilder.Entity<RecipeTag>(entity =>
        {
            entity.HasKey(rt => new { rt.RecipeId, rt.TagId });

            entity.HasOne(rt => rt.Recipe)
                .WithMany(r => r.RecipeTags)
                .HasForeignKey(rt => rt.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rt => rt.Tag)
                .WithMany()
                .HasForeignKey(rt => rt.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeIngredient>(entity =>
        {
            entity.HasKey(ri => ri.Id);
            entity.Property(ri => ri.Id).ValueGeneratedNever();

            entity.Property(ri => ri.FreeText).HasMaxLength(200);
            entity.Property(ri => ri.Amount).HasPrecision(18, 4);
            entity.Property(ri => ri.Unit).HasConversion<string>().HasMaxLength(20);
            entity.Property(ri => ri.CustomUnitDescription).HasMaxLength(50);
            entity.Property(ri => ri.SortOrder).HasDefaultValue(0);

            entity.HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ri => ri.Ingredient)
                .WithMany()
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });

        modelBuilder.Entity<MenuEntry>(entity =>
        {
            entity.HasIndex(e => e.Date).IsUnique();
            entity.Property(e => e.DeliveryNote).HasMaxLength(500);

            entity.HasOne(e => e.Recipe)
                .WithMany()
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // Reuse for every entity that inherits BaseEntity: GUID PK (non-clustered) + SysId clustered index
    private static void ConfigureBaseEntity<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.Property(e => e.SysId)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            entity.HasIndex(e => e.SysId)
                .IsUnique()
                .IsClustered();
        });
    }
}
