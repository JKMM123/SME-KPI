using SmeKpiDashboard.Models;

namespace SmeKpiDashboard.Repositories;

public interface IExpenseRepository
{
    Task<List<Expense>> GetAllByUserIdAsync(Guid userId);
    Task<Expense?> GetByIdAsync(Guid id, Guid userId);
    Task<Expense> CreateAsync(Expense expense);
    Task<Expense> UpdateAsync(Expense expense);
    Task DeleteAsync(Guid id, Guid userId);
}
