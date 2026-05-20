using SmeKpiDashboard.DTOs;

namespace SmeKpiDashboard.Repositories;

public interface IDashboardRepository
{
    Task<KpiSummaryResponse> GetKpiSummaryAsync(Guid userId);
    Task<ChartDataResponse> GetChartDataAsync(Guid userId);
    Task<MonthlyReportResponse> GetMonthlyReportAsync(Guid userId, int year, int month);
    Task<List<ExpenseByCategoryResponse>> GetExpenseDistributionAsync(Guid userId);
}
