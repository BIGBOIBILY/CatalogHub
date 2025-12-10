using CatalogHub.Domain.Interfaces.Repository;
using CatalogHub.Domain.Models;
using CatalogHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogHub.Infrastructure.Repository;

public class ProductRepository(CatalogHubDbContext context) : IProductRepository
{
    public async Task<Product?> CreateAsync(Product product)
    {
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> DeleteAsync(Product product)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
        => await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<(List<Product> products, int totalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name);

        var totalCount = await query.CountAsync();

        var products = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }

    public async Task<List<Product>?> GetByFiltersAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, bool? isActive)
    {
        var query = context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }
}
