using Microsoft.EntityFrameworkCore;
using MijnKeuken.Domain.Entities;

namespace MijnKeuken.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureBaseEntity<User>(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
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
                .UseIdentityColumn();

            entity.HasIndex(e => e.SysId)
                .IsUnique()
                .IsClustered();
        });
    }
}
