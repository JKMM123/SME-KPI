using SmeKpiDashboard.Models;

namespace SmeKpiDashboard.Repositories;

public interface ISaleRepository
{
    Task<List<Sale>> GetAllByUserIdAsync(Guid userId);
    Task<Sale?> GetByIdAsync(Guid id, Guid userId);
    Task<Sale> CreateAsync(Sale sale);
    Task<Sale> UpdateAsync(Sale sale);
    Task DeleteAsync(Guid id, Guid userId);
}
