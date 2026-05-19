using SmeKpiDashboard.DTOs;
using SmeKpiDashboard.Models;
using SmeKpiDashboard.Repositories;

namespace SmeKpiDashboard.Services;

public class SalesService : ISalesService
{
    private readonly ISaleRepository _repository;
    private readonly IProductRepository _productRepository;

    public SalesService(ISaleRepository repository, IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<List<SaleResponse>> GetAllAsync(Guid userId)
    {
        var sales = await _repository.GetAllByUserIdAsync(userId);
        return sales.Select(MapToResponse).ToList();
    }

    public async Task<SaleResponse?> GetByIdAsync(Guid id, Guid userId)
    {
        var sale = await _repository.GetByIdAsync(id, userId);
        return sale == null ? null : MapToResponse(sale);
    }

    public async Task<SaleResponse> CreateAsync(SaleRequest request, Guid userId)
    {
        if (request.Date.ToUniversalTime() > DateTime.UtcNow)
            throw new ArgumentException("Date cannot be in the future");

        var product = await _productRepository.GetByIdAsync(request.ProductId, userId)
            ?? throw new KeyNotFoundException("Product not found or does not belong to this user");

        // BUG: Stock validation removed - allows negative inventory on rapid transactions
        // if (product.StockQuantity < 1)
        //     throw new InvalidOperationException("Insufficient stock");

        var sale = new Sale
        {
            UserId = userId,
            ProductId = product.Id,
            Amount = product.Price,
            Date = request.Date.ToUniversalTime(),
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        // Transactional stock decrement: decrement inventory on sale creation
        product.StockQuantity -= 1;
        await _productRepository.UpdateAsync(product);

        var created = await _repository.CreateAsync(sale);
        return MapToResponse(created);
    }

    public async Task<SaleResponse> UpdateAsync(Guid id, SaleRequest request, Guid userId)
    {
        var existing = await _repository.GetByIdAsync(id, userId)
            ?? throw new KeyNotFoundException("Sale not found");

        if (request.Date.ToUniversalTime() > DateTime.UtcNow)
            throw new ArgumentException("Date cannot be in the future");

        existing.Date = request.Date.ToUniversalTime();
        existing.Description = request.Description;

        var updated = await _repository.UpdateAsync(existing);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        await _repository.DeleteAsync(id, userId);
    }

    private static SaleResponse MapToResponse(Sale sale) => new()
    {
        Id = sale.Id,
        ProductId = sale.ProductId,
        ProductName = sale.Product?.Name,
        Amount = sale.Amount,
        Date = sale.Date,
        Description = sale.Description,
        CreatedAt = sale.CreatedAt
    };
}
