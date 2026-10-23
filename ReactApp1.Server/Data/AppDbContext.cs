using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Models;

namespace ReactApp1.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Login).IsRequired().HasMaxLength(100);

            entity.HasIndex(u => u.Login).IsUnique();

            entity.Property(u => u.PasswordHash).IsRequired();

            entity.Property(u => u.PasswordSalt).IsRequired();
        });
    }
}
