using CatalogHub.Domain.Models;

namespace CatalogHub.Domain.Interfaces.Repository;

public interface ICategoryRepository
{
    Task<Category?> CreateAsync(Category category);
    Task<Category?> UpdateAsync(Category category);
    Task<Category?> DeleteAsync(Category category);
    Task<Category?> GetByIdAsync(Guid id);
    Task<(List<Category> categories, int totalCount)> GetAllAsync(int pageNumber, int pageSize);
}
