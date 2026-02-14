namespace Tabibi.API.DTOs.DoctorSystem
{
    public sealed record EarningsSummaryDto(
        decimal TotalLifetimeEarnings,
        decimal ThisMonthEarnings,
        decimal AppCommission,
        double GrowthPercentage,
        int TotalConsultations,
        decimal AveragePerVisit,
        List<ChartDataPointDto> WeeklyChartData,
        List<TransactionItemDto> RecentTransactions
    );
}
