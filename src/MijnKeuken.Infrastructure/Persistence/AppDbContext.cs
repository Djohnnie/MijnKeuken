using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureBaseEntity<User>(modelBuilder);
        ConfigureBaseEntity<Tag>(modelBuilder);
        ConfigureBaseEntity<StorageLocation>(modelBuilder);

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
