using Hangfire.Dashboard;

namespace QuilvianSystemBackendDev.Hangfire.Controllers
{
    public class HangfireDashboardAuthFilterController : IDashboardAuthorizationFilter
    {

        public bool Authorize(DashboardContext context) => true;
        //public bool Authorize(DashboardContext context)
        //{
        //    var http = context.GetHttpContext();

        //    return http.User.Identity?.IsAuthenticated == true
        //           && http.User.IsInRole("SuperAdmin");
        //}
    }
}
