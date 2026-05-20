using SmeKpiDashboard.DTOs;
using SmeKpiDashboard.Models;
using SmeKpiDashboard.Repositories;

namespace SmeKpiDashboard.Services;

public class CustomersService : ICustomersService
{
    private readonly ICustomerRepository _repository;

    public CustomersService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CustomerResponse>> GetAllAsync(Guid userId)
    {
        var customers = await _repository.GetAllByUserIdAsync(userId);
        return customers.Select(MapToResponse).ToList();
    }

    public async Task<CustomerResponse?> GetByIdAsync(Guid id, Guid userId)
    {
        var customer = await _repository.GetByIdAsync(id, userId);
        return customer == null ? null : MapToResponse(customer);
    }

    public async Task<CustomerResponse> CreateAsync(CustomerRequest request, Guid userId)
    {
        var customer = new Customer
        {
            UserId = userId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(customer);
        return MapToResponse(created);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, CustomerRequest request, Guid userId)
    {
        var existing = await _repository.GetByIdAsync(id, userId)
            ?? throw new KeyNotFoundException("Customer not found");

        existing.Name = request.Name;
        existing.Email = request.Email;
        existing.Phone = request.Phone;

        var updated = await _repository.UpdateAsync(existing);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        await _repository.DeleteAsync(id, userId);
    }

    private static CustomerResponse MapToResponse(Customer customer) => new()
    {
        Id = customer.Id,
        Name = customer.Name,
        Email = customer.Email,
        Phone = customer.Phone,
        CreatedAt = customer.CreatedAt
    };
}
