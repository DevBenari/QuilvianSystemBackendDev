using System.Security.Claims;

namespace QuilvianSystem.Areas.UserManagement.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserManagementService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string GetUserId()
        {
            return _contextAccessor.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
