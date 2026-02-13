using Hangfire.Dashboard;

namespace OrbWeaver.Host;

public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}