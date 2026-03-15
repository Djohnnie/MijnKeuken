using MediatR;
using MijnKeuken.Application.Dashboard.DTOs;

namespace MijnKeuken.Application.Dashboard.Queries.GetDashboardStats;

public record GetDashboardStatsQuery(int TopCount = 5) : IRequest<DashboardStatsDto>;
