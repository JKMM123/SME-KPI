using SmeKpiDashboard.DTOs;

namespace SmeKpiDashboard.Services;

public interface ISalesService
{
    Task<List<SaleResponse>> GetAllAsync(Guid userId);
    Task<SaleResponse?> GetByIdAsync(Guid id, Guid userId);
    Task<SaleResponse> CreateAsync(SaleRequest request, Guid userId);
    Task<SaleResponse> UpdateAsync(Guid id, SaleRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
