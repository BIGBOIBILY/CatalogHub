using CatalogHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogHub.Infrastructure.Data;

public class CatalogHubDbContext : DbContext
{
    public CatalogHubDbContext(DbContextOptions<CatalogHubDbContext> options)
        : base(options)
    { }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogHubDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
