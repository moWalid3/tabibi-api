namespace Tabibi.API.DTOs.AdminDashboard
{
    public sealed record AdminDashboardSummaryDto
    {
        public int TotalDoctors { get; init; }
        public int TotalPendingDoctors { get; init; }
        public int TotalApprovedDoctors { get; init; }
        public int TotalPatients { get; init; }
    }
}
