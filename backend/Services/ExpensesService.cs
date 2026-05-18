using SmeKpiDashboard.DTOs;
using SmeKpiDashboard.Models;
using SmeKpiDashboard.Repositories;

namespace SmeKpiDashboard.Services;

public class ExpensesService : IExpensesService
{
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Operating Costs", "Marketing", "Salaries", "Inventory", "Utilities", "Other"
    };

    private readonly IExpenseRepository _repository;

    public ExpensesService(IExpenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ExpenseResponse>> GetAllAsync(Guid userId)
    {
        var expenses = await _repository.GetAllByUserIdAsync(userId);
        return expenses.Select(MapToResponse).ToList();
    }

    public async Task<ExpenseResponse?> GetByIdAsync(Guid id, Guid userId)
    {
        var expense = await _repository.GetByIdAsync(id, userId);
        return expense == null ? null : MapToResponse(expense);
    }

    public async Task<ExpenseResponse> CreateAsync(ExpenseRequest request, Guid userId)
    {
        if (request.Date.ToUniversalTime() > DateTime.UtcNow)
            throw new ArgumentException("Date cannot be in the future");

        if (!AllowedCategories.Contains(request.Category))
            throw new ArgumentException($"Invalid category. Allowed values: {string.Join(", ", AllowedCategories)}");

        var expense = new Expense
        {
            UserId = userId,
            Amount = request.Amount,
            Date = request.Date.ToUniversalTime(),
            Category = request.Category,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(expense);
        return MapToResponse(created);
    }

    public async Task<ExpenseResponse> UpdateAsync(Guid id, ExpenseRequest request, Guid userId)
    {
        var existing = await _repository.GetByIdAsync(id, userId)
            ?? throw new KeyNotFoundException("Expense not found");

        if (request.Date.ToUniversalTime() > DateTime.UtcNow)
            throw new ArgumentException("Date cannot be in the future");

        if (!AllowedCategories.Contains(request.Category))
            throw new ArgumentException($"Invalid category. Allowed values: {string.Join(", ", AllowedCategories)}");

        existing.Amount = request.Amount;
        existing.Date = request.Date.ToUniversalTime();
        existing.Category = request.Category;
        existing.Description = request.Description;

        var updated = await _repository.UpdateAsync(existing);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        await _repository.DeleteAsync(id, userId);
    }

    private static ExpenseResponse MapToResponse(Expense expense) => new()
    {
        Id = expense.Id,
        Amount = expense.Amount,
        Date = expense.Date,
        Category = expense.Category,
        Description = expense.Description,
        CreatedAt = expense.CreatedAt
    };
}
