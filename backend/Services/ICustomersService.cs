using SmeKpiDashboard.DTOs;

namespace SmeKpiDashboard.Services;

public interface ICustomersService
{
    Task<List<CustomerResponse>> GetAllAsync(Guid userId);
    Task<CustomerResponse?> GetByIdAsync(Guid id, Guid userId);
    Task<CustomerResponse> CreateAsync(CustomerRequest request, Guid userId);
    Task<CustomerResponse> UpdateAsync(Guid id, CustomerRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
