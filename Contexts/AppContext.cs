using FilesAPI.Contexts;
using Microsoft.EntityFrameworkCore;

public class AppContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserFile> Files { get; set; } = null!;
    public AppContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=filesdb;Username=postgres;Password=1");
    }
}