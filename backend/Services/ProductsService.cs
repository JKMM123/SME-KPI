using SmeKpiDashboard.DTOs;
using SmeKpiDashboard.Models;
using SmeKpiDashboard.Repositories;

namespace SmeKpiDashboard.Services;

public class ProductsService : IProductsService
{
    private readonly IProductRepository _repository;

    public ProductsService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductResponse>> GetAllAsync(Guid userId)
    {
        var products = await _repository.GetAllByUserIdAsync(userId);
        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, Guid userId)
    {
        var product = await _repository.GetByIdAsync(id, userId);
        return product == null ? null : MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request, Guid userId)
    {
        var product = new Product
        {
            UserId = userId,
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(product);
        return MapToResponse(created);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, ProductRequest request, Guid userId)
    {
        var existing = await _repository.GetByIdAsync(id, userId)
            ?? throw new KeyNotFoundException("Product not found");

        existing.Name = request.Name;
        existing.Price = request.Price;
        existing.StockQuantity = request.StockQuantity;

        var updated = await _repository.UpdateAsync(existing);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        await _repository.DeleteAsync(id, userId);
    }

    private static ProductResponse MapToResponse(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Price = product.Price,
        StockQuantity = product.StockQuantity,
        CreatedAt = product.CreatedAt
    };
}
