
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuilvianSystem.Areas.UserManagement.ViewModels
{
    public class ChangeUserManagementViewModel
    {
        public IList<SelectListItem> Roles { get; set; }
    }
}
