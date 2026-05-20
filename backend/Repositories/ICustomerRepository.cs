using SmeKpiDashboard.Models;

namespace SmeKpiDashboard.Repositories;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllByUserIdAsync(Guid userId);
    Task<Customer?> GetByIdAsync(Guid id, Guid userId);
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer> UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id, Guid userId);
}
