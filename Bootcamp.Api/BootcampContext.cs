using Bootcamp.Api.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace Bootcamp.Api;

public class BootcampContext(DbContextOptions<BootcampContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}