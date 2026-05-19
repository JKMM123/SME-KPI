using SmeKpiDashboard.DTOs;

namespace SmeKpiDashboard.Services;

public interface IProductsService
{
    Task<List<ProductResponse>> GetAllAsync(Guid userId);
    Task<ProductResponse?> GetByIdAsync(Guid id, Guid userId);
    Task<ProductResponse> CreateAsync(ProductRequest request, Guid userId);
    Task<ProductResponse> UpdateAsync(Guid id, ProductRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
