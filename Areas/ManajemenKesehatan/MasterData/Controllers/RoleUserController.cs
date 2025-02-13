//using Microsoft.AspNetCore.DataProtection;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using QuilvianSystemBackendDev.Models;
//using QuilvianSystemBackendDev.Repositories;

//namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
//{
//    public class RoleUserController : Controller
//    {
//        private readonly RoleManager<IdentityRole> _roleManager;
//        private readonly ApplicationDbContext _applicationDbContext;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        //private readonly IPositionRepository _positionRepository;

//        private readonly IDataProtector _protector;

//        public RoleUserController(ApplicationDbContext applicationDbContext,
//            SignInManager<ApplicationUser> signInManager,
//            RoleManager<IdentityRole> roleManager,
//            //IPositionRepository PositionRepository,

//            IDataProtectionProvider provider
//        )
//        {
//            _applicationDbContext = applicationDbContext;
//            _roleManager = roleManager;
//            _signInManager = signInManager;
//            //_positionRepository = PositionRepository;

//            _protector = provider.CreateProtector("UrlProtector");
//        }
//        public IActionResult LoadRoles(string Position)
//        {
//            ViewBag.Active = "Administrator";
//            if (!string.IsNullOrEmpty(Position))
//            {
//                //var userId = _positionRepository.GetAllPosition()
//                //    .FirstOrDefault(u => u.PositionCode == Position);
//                var userId = "";
//                // Mengambil bukan DepartemenId
//                var roleIdsNotForDep = _applicationDbContext.RolePositions
//                .Where(gr => gr.PositionId == userId.PositionId.ToString())
//                .Select(gr => gr.RoleId)
//                .ToList();

//                var rolesNotForDep = _roleManager.Roles
//                    .Where(role => roleIdsNotForDep.Contains(role.Id))
//                    .OrderBy(role => role.ConcurrencyStamp)
//                    .ToList();

//                // Filter roles yang hanya ada di roleIds dan bukan di roleIdsNotForDep
//                var allRoles = _roleManager.Roles.ToList();
//                var rolesForDep = allRoles
//                    .Where(role => !roleIdsNotForDep.Contains(role.Id))
//                    .OrderBy(role => role.ConcurrencyStamp)
//                    .ToList();

//                var result = new
//                {
//                    RolesForDepartment = rolesForDep,
//                    RolesNotForDepartment = rolesNotForDep
//                };

//                return Json(result);
//            }
//            else
//            {
//                var roles = _roleManager.Roles
//                    .OrderBy(role => role.ConcurrencyStamp)
//                    .ToList();
//                return Json(new
//                {
//                    RolesForDepartment = roles
//                });
//            }
//        }
//    }
//}
