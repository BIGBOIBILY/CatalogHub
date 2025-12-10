using CatalogHub.Domain.Models;

namespace CatalogHub.Domain.Interfaces.Repository;

public interface IProductRepository
{
    Task<Product?> CreateAsync(Product product);
    Task<Product?> UpdateAsync(Product product);
    Task<Product?> DeleteAsync(Product product);
    Task<Product?> GetByIdAsync(Guid id);
    Task<(List<Product> products, int totalCount)> GetAllAsync(int pageNumber, int pageSize);
    Task<List<Product>?> GetByFiltersAsync(Guid? categoryId, decimal? minPrice, decimal? maxPrice, bool? isActive);
}