using CatalogHub.Domain.Interfaces.Repository;
using CatalogHub.Domain.Models;
using CatalogHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogHub.Infrastructure.Repository;

public class CategoryRepository(CatalogHubDbContext context) : ICategoryRepository
{
    public async Task<Category?> CreateAsync(Category category)
    {
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateAsync(Category category)
    {
        context.Categories.Update(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> DeleteAsync(Category category)
    {
        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> GetByIdAsync(Guid id)
        => await context.Categories
            .AsNoTracking()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<(List<Category> categories, int totalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = context.Categories.AsNoTracking().OrderBy(c => c.Name);

        var totalCount = await query.CountAsync();

        var categories = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (categories, totalCount);
    }
}
