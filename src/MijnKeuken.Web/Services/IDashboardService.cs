using MijnKeuken.Application.Dashboard.DTOs;

namespace MijnKeuken.Web.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(int top = 5);
}
