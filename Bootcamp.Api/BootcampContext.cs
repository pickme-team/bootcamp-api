using Microsoft.EntityFrameworkCore;

namespace Bootcamp.Api;

public class BootcampContext(DbContextOptions<BootcampContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}