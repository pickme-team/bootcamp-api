using Bootcamp.Api.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace Bootcamp.Api.Services;

public class BootcampContext(DbContextOptions<BootcampContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Job> Jobs { get; set; }
    
    public DbSet<JobExec> JobExecs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        
        modelBuilder.Entity<JobExec>().HasIndex(j => j.JobId).IsUnique();
    }
}