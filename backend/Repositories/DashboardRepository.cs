using Microsoft.EntityFrameworkCore;
using SmeKpiDashboard.Data;
using SmeKpiDashboard.DTOs;

namespace SmeKpiDashboard.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<KpiSummaryResponse> GetKpiSummaryAsync(Guid userId)
    {
        var totalRevenue = await _context.Sales
            .Where(s => s.UserId == userId)
            .SumAsync(s => (decimal?)s.Amount) ?? 0;

        var totalExpenses = await _context.Expenses
            .Where(e => e.UserId == userId)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var now = DateTime.UtcNow;
        var thisMonthStart = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var thisMonthRevenue = await _context.Sales
            .Where(s => s.UserId == userId && s.Date >= thisMonthStart)
            .SumAsync(s => (decimal?)s.Amount) ?? 0;

        var lastMonthRevenue = await _context.Sales
            .Where(s => s.UserId == userId && s.Date >= lastMonthStart && s.Date < thisMonthStart)
            .SumAsync(s => (decimal?)s.Amount) ?? 0;

        var growth = lastMonthRevenue > 0
            ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
            : 0;

        // Top selling product (by sales count)
        var topProductGroup = await _context.Sales
            .Where(s => s.UserId == userId && s.ProductId != null)
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .FirstOrDefaultAsync();

        string? topProductName = null;
        if (topProductGroup?.ProductId != null)
        {
            topProductName = await _context.Products
                .Where(p => p.Id == topProductGroup.ProductId && p.UserId == userId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync();
        }

        var lowStockCount = await _context.Products
            .Where(p => p.UserId == userId && p.StockQuantity < 5)
            .CountAsync();

        return new KpiSummaryResponse
        {
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            NetProfit = totalRevenue - totalExpenses,
            GrowthPercentage = Math.Round(growth, 2),
            TopSellingProductName = topProductName,
            LowStockAlertCount = lowStockCount
        };
    }

    public async Task<ChartDataResponse> GetChartDataAsync(Guid userId)
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var sales = await _context.Sales
            .Where(s => s.UserId == userId && s.Date >= sixMonthsAgo)
            .GroupBy(s => new { s.Date.Year, s.Date.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(s => s.Amount) })
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(e => e.UserId == userId && e.Date >= sixMonthsAgo)
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(e => e.Amount) })
            .ToListAsync();

        var monthlyData = new List<MonthlyDataPoint>();
        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddMonths(-i);
            var salesTotal = sales.FirstOrDefault(s => s.Year == date.Year && s.Month == date.Month)?.Total ?? 0;
            var expensesTotal = expenses.FirstOrDefault(e => e.Year == date.Year && e.Month == date.Month)?.Total ?? 0;

            monthlyData.Add(new MonthlyDataPoint
            {
                Month = date.ToString("MMM yyyy"),
                Sales = salesTotal,
                Expenses = expensesTotal,
                Profit = salesTotal - expensesTotal
            });
        }

        return new ChartDataResponse { MonthlyData = monthlyData };
    }

    public async Task<MonthlyReportResponse> GetMonthlyReportAsync(Guid userId, int year, int month)
    {
        var startDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1);

        var totalSales = await _context.Sales
            .Where(s => s.UserId == userId && s.Date >= startDate && s.Date < endDate)
            .SumAsync(s => (decimal?)s.Amount) ?? 0;

        var totalExpenses = await _context.Expenses
            .Where(e => e.UserId == userId && e.Date >= startDate && e.Date < endDate)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var expensesByCategory = await _context.Expenses
            .Where(e => e.UserId == userId && e.Date >= startDate && e.Date < endDate)
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseByCategoryResponse
            {
                Category = g.Key,
                Total = g.Sum(e => e.Amount)
            })
            .ToListAsync();

        return new MonthlyReportResponse
        {
            Year = year,
            Month = month,
            TotalSales = totalSales,
            TotalExpenses = totalExpenses,
            NetProfit = totalSales - totalExpenses,
            ExpensesByCategory = expensesByCategory
        };
    }

    public async Task<List<ExpenseByCategoryResponse>> GetExpenseDistributionAsync(Guid userId)
    {
        return await _context.Expenses
            .Where(e => e.UserId == userId)
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseByCategoryResponse
            {
                Category = g.Key,
                Total = g.Sum(e => e.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();
    }
}
