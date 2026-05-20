using SmeKpiDashboard.DTOs;

namespace SmeKpiDashboard.Services;

public interface IExpensesService
{
    Task<List<ExpenseResponse>> GetAllAsync(Guid userId);
    Task<ExpenseResponse?> GetByIdAsync(Guid id, Guid userId);
    Task<ExpenseResponse> CreateAsync(ExpenseRequest request, Guid userId);
    Task<ExpenseResponse> UpdateAsync(Guid id, ExpenseRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
