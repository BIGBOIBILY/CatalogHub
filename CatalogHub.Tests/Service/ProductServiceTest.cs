using CatalogHub.Application.Common.Pagination;
using CatalogHub.Application.DTOs.Product;
using CatalogHub.Application.Interfaces.Services;
using CatalogHub.Application.Services;
using CatalogHub.Domain.Interfaces.Repository;
using CatalogHub.Domain.Models;
using Moq;

namespace CatalogHub.Tests.Service;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<ICategoryRepository> _mockCategoryRepo;
    private readonly Mock<IStorageService> _mockStorage;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockProductRepo = new Mock<IProductRepository>();
        _mockCategoryRepo = new Mock<ICategoryRepository>();
        _mockStorage = new Mock<IStorageService>();

        _service = new ProductService(
            _mockProductRepo.Object,
            _mockCategoryRepo.Object,
            _mockStorage.Object
        );
    }

    [Fact(DisplayName = "Should create product successfully")]
    public async Task Should_Create_Product_Successfully()
    {
        var categoryId = Guid.NewGuid();

        var request = new CreateProductRequest
        {
            Name = "Produto Teste",
            Description = "Descrição Teste",
            Price = 100m,
            StockQuantity = 10,
            CategoryId = categoryId,
            IsActive = true
        };

        _mockCategoryRepo
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Id = categoryId, Name = "Categoria Teste" });

        _mockStorage
            .Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync("https://fakeurl.com/image.png");

        _mockProductRepo
            .Setup(r => r.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        var result = await _service.CreateAsync(request, null, null, null);

        Assert.NotNull(result);
        Assert.Equal(request.Name, result!.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.Price, result.Price);
        Assert.Equal(request.StockQuantity, result.StockQuantity);
        Assert.Equal(request.CategoryId, result.CategoryId);

        _mockProductRepo.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
        _mockCategoryRepo.Verify(r => r.GetByIdAsync(categoryId), Times.AtLeastOnce());
    }

    [Fact(DisplayName = "Should not create product with invalid category")]
    public async Task Should_Not_Create_Product_With_Invalid_Category()
    {
        var request = new CreateProductRequest
        {
            Name = "Invalid Category Product",
            Description = "Description",
            Price = 50,
            StockQuantity = 5,
            CategoryId = Guid.NewGuid(),
            IsActive = true
        };

        _mockCategoryRepo.Setup(r => r.GetByIdAsync(request.CategoryId))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request, null, "fake.png", null));
    }

    [Fact(DisplayName = "Should update product successfully")]
    public async Task Should_Update_Product_Successfully()
    {
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var existingProduct = new Product
        {
            Id = productId,
            Name = "Old Name",
            Description = "Old Description",
            Price = 50,
            StockQuantity = 5,
            IsActive = true,
            CategoryId = categoryId
        };

        var updateRequest = new UpdateProductRequest
        {
            Id = productId,
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 99.99m,
            StockQuantity = 15,
            IsActive = false,
            CategoryId = categoryId,
            RemoveImage = false
        };

        _mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(existingProduct);
        _mockCategoryRepo.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(new Category { Id = categoryId, Name = "Category A" });
        _mockProductRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync((Product p) => p);

        var result = await _service.UpdateAsync(updateRequest, null, null, null);

        Assert.NotNull(result);
        Assert.Equal("Updated Product", result.Name);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal(15, result.StockQuantity);
        Assert.False(result.IsActive);
        Assert.Equal(categoryId, result.CategoryId);

        _mockProductRepo.Verify(r => r.GetByIdAsync(productId), Times.Once);
        _mockProductRepo.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Name == "Updated Product")), Times.Once);
        _mockCategoryRepo.Verify(r => r.GetByIdAsync(categoryId), Times.AtLeastOnce());
    }

    [Fact(DisplayName = "Should throw when updating non-existing product")]
    public async Task Should_Throw_When_Updating_NonExisting_Product()
    {
        var updateRequest = new UpdateProductRequest
        {
            Id = Guid.NewGuid(),
            Name = "Name",
            Description = "Description",
            Price = 10,
            StockQuantity = 1,
            CategoryId = Guid.NewGuid(),
            IsActive = true
        };

        _mockProductRepo.Setup(r => r.GetByIdAsync(updateRequest.Id))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateAsync(updateRequest, null, null, null));
    }

    [Fact(DisplayName = "Should delete product successfully")]
    public async Task Should_Delete_Product_Successfully()
    {
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Product",
            Description = "Description",
            Price = 50,
            StockQuantity = 3,
            IsActive = true,
            CategoryId = Guid.NewGuid()
        };

        _mockProductRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(existingProduct);
        _mockProductRepo.Setup(r => r.DeleteAsync(existingProduct))
            .ReturnsAsync(existingProduct);

        var result = await _service.DeleteAsync(productId);

        Assert.NotNull(result);
        Assert.Equal(productId, result!.Id);
    }

    [Fact(DisplayName = "Should throw when deleting non-existing product")]
    public async Task Should_Throw_When_Deleting_NonExisting_Product()
    {
        var productId = Guid.NewGuid();

        _mockProductRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.DeleteAsync(productId));
    }

    [Fact(DisplayName = "Should get product by id successfully")]
    public async Task GetByIdAsync_ShouldReturnProduct()
    {
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Produto Teste",
            Description = "Descrição Teste",
            Price = 100,
            StockQuantity = 10,
            IsActive = true,
            CategoryId = Guid.NewGuid()
        };

        _mockProductRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _mockCategoryRepo.Setup(r => r.GetByIdAsync(product.CategoryId))
            .ReturnsAsync(new Category { Id = product.CategoryId, Name = "Categoria Teste" });

        var result = await _service.GetByIdAsync(productId);

        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Categoria Teste", result.CategoryName);
    }

    [Fact(DisplayName = "Should return null when product by id does not exist")]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var productId = Guid.NewGuid();

        _mockProductRepo.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(productId);

        Assert.Null(result);
    }

    [Fact(DisplayName = "Should get products by filters successfully")]
    public async Task GetByFiltersAsync_ShouldReturnFilteredProducts()
    {
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
    {
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produto 1",
            Price = 50,
            StockQuantity = 5,
            IsActive = true,
            CategoryId = categoryId
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Produto 2",
            Price = 100,
            StockQuantity = 10,
            IsActive = true,
            CategoryId = categoryId
        }
    };

        _mockProductRepo.Setup(r => r.GetByFiltersAsync(categoryId, null, null, true))
            .ReturnsAsync(products);

        _mockCategoryRepo.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Id = categoryId, Name = "Categoria Teste" });

        var result = await _service.GetByFiltersAsync(categoryId, null, null, true);

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal("Categoria Teste", p.CategoryName));
    }

    [Fact(DisplayName = "Should return empty list when no products match filters")]
    public async Task GetByFiltersAsync_ShouldReturnEmptyList_WhenNoMatch()
    {
        var categoryId = Guid.NewGuid();

        _mockProductRepo.Setup(r => r.GetByFiltersAsync(categoryId, 1000, 2000, true))
            .ReturnsAsync(new List<Product>());

        var result = await _service.GetByFiltersAsync(categoryId, 1000, 2000, true);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "Should get all products successfully with pagination")]
    public async Task GetAllAsync_ShouldReturnPaginatedProducts()
    {
        var paginationRequest = new PaginationRequest { PageNumber = 1, PageSize = 10 };
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Prod 1", CategoryId = categoryId, Category = new Category { Id = categoryId, Name = "Test Cat" } },
            new() { Id = Guid.NewGuid(), Name = "Prod 2", CategoryId = categoryId, Category = new Category { Id = categoryId, Name = "Test Cat" } }
        };
        var totalCount = products.Count;

        _mockProductRepo.Setup(r => r.GetAllAsync(paginationRequest.PageNumber, paginationRequest.PageSize))
            .ReturnsAsync((products, totalCount));

        _mockCategoryRepo.Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Id = categoryId, Name = "Test Cat" });

        var result = await _service.GetAllAsync(paginationRequest);

        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PageSize);
        Assert.Equal("Test Cat", result.Data[0].CategoryName);
    }
}