using Microsoft.EntityFrameworkCore;

namespace GameHubApi.DataBase;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options) { }

    public virtual DbSet<User>? Users { get; set; }
    public virtual DbSet<Game>? Games { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Make UserName unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique(true);

        // Make Game name unique
        modelBuilder.Entity<Game>()
            .HasIndex(g => g.Name)
            .IsUnique(true);
    }
}

