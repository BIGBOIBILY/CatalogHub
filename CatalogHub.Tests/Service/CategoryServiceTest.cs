using CatalogHub.Application.Common.Pagination;
using CatalogHub.Application.DTOs.Category;
using CatalogHub.Application.Services;
using CatalogHub.Domain.Interfaces.Repository;
using CatalogHub.Domain.Models;
using Moq;

namespace CatalogHub.Tests.Service;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockRepo;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _mockRepo = new Mock<ICategoryRepository>();
        _service = new CategoryService(_mockRepo.Object);
    }

    [Fact(DisplayName = "Should create category successfully")]
    public async Task CreateAsync_ShouldSucceed()
    {
        var request = new CreateCategoryRequest
        {
            Name = "Categoria Teste",
            Description = "Descrição Teste"
        };

        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => c);

        var result = await _service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Categoria Teste", result.Name);
        _mockRepo.Verify(r => r.CreateAsync(It.Is<Category>(c => c.Name == "Categoria Teste")), Times.Once);
    }

    [Fact(DisplayName = "Should update category successfully")]
    public async Task UpdateAsync_ShouldSucceed()
    {
        var existing = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            Description = "Old Desc"
        };

        var request = new UpdateCategoryRequest
        {
            Id = existing.Id,
            Name = "New Name",
            Description = "New Desc"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(existing.Id))
            .ReturnsAsync(existing);

        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => c);

        var result = await _service.UpdateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        _mockRepo.Verify(r => r.GetByIdAsync(existing.Id), Times.Once);
        _mockRepo.Verify(r => r.UpdateAsync(It.Is<Category>(c => c.Name == "New Name")), Times.Once);
    }

    [Fact(DisplayName = "Should throw exception when updating non-existent category")]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        var request = new UpdateCategoryRequest
        {
            Id = Guid.NewGuid(),
            Name = "New Name",
            Description = "New Desc"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(request.Id))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(request));
    }

    [Fact(DisplayName = "Should delete category successfully")]
    public async Task DeleteAsync_ShouldSucceed()
    {
        var existing = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Categoria Teste"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(existing.Id))
            .ReturnsAsync(existing);

        _mockRepo.Setup(r => r.DeleteAsync(existing))
            .ReturnsAsync(existing);

        var result = await _service.DeleteAsync(existing.Id);

        Assert.NotNull(result);
        Assert.Equal(existing.Id, result.Id);
        _mockRepo.Verify(r => r.DeleteAsync(existing), Times.Once);
    }

    [Fact(DisplayName = "Should throw exception when deleting non-existent category")]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        var categoryId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(categoryId));
    }

    [Fact(DisplayName = "Should get all categories successfully with pagination")]
    public async Task GetAllAsync_ShouldReturnPaginatedCategories()
    {
        var paginationRequest = new PaginationRequest { PageNumber = 1, PageSize = 10 };
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Cat 1" },
            new() { Id = Guid.NewGuid(), Name = "Cat 2" }
        };
        var totalCount = categories.Count;

        _mockRepo.Setup(r => r.GetAllAsync(paginationRequest.PageNumber, paginationRequest.PageSize))
            .ReturnsAsync((categories, totalCount));

        var result = await _service.GetAllAsync(paginationRequest);

        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PageSize);
        Assert.False(result.HasNext);
        Assert.Contains(result.Data, c => c.Name == "Cat 1");
    }

    [Fact(DisplayName = "Should get category by id successfully")]
    public async Task GetByIdAsync_ShouldReturnCategory()
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Categoria Teste"
        };

        _mockRepo.Setup(r => r.GetByIdAsync(category.Id))
            .ReturnsAsync(category);

        var result = await _service.GetByIdAsync(category.Id);

        Assert.NotNull(result);
        Assert.Equal("Categoria Teste", result.Name);
    }

    [Fact(DisplayName = "Should return null when category by id does not exist")]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var categoryId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync((Category?)null);

        var result = await _service.GetByIdAsync(categoryId);

        Assert.Null(result);
    }
}
