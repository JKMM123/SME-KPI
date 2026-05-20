using SmeKpiDashboard.Models;

namespace SmeKpiDashboard.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllByUserIdAsync(Guid userId);
    Task<Product?> GetByIdAsync(Guid id, Guid userId);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Guid id, Guid userId);
}
