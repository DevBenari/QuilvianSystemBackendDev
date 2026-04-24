using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Services;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    //[EnableCors("AllowSpecific")]
    public class RoleDepartemenController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly NFCReaderService _nfcService;

        // Constructor untuk Dependency Injection
        public RoleDepartemenController(RoleManager<IdentityRole> roleManager, ApplicationDbContext applicationDbContext, NFCReaderService nfcService)
        {
            _roleManager = roleManager;
            _applicationDbContext = applicationDbContext;
            _nfcService = nfcService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            if (allRoles == null || !allRoles.Any())
            {
                return NotFound(new { message = "Belum ada data. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = allRoles
            });
        }

        // Created Role Ke Table
        [HttpPost("AllRole")]
        public async Task<IActionResult> CreateRoles()
        {
            // Ambil semua controller dalam aplikasi
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type) && !type.IsAbstract)
                .ToList();

            foreach (var controllerType in controllers)
            {
                // Mengambil nama controller tanpa "Controller"
                var controllerName = controllerType.Name.Replace("Controller", "");

                // Menangkap nama area dan sub-area dari namespace
                var namespaceParts = controllerType.Namespace?.Split('.');

                // Periksa jika namespace memiliki lebih dari dua bagian, yang menunjukkan adanya Area dan Sub-Area
                if (namespaceParts?.Length > 2)
                {
                    // Area Name adalah bagian kedua dari namespace
                    var areaName = namespaceParts[1]; // Misalnya "ManajemenKesehatan"

                    // Sub-Area Name adalah bagian ketiga dari namespace
                    var subAreaName = namespaceParts.Length > 3 ? namespaceParts[2] : null; // Misalnya "Master" atau "Pendaftaran"

                    // Ambil aksi (method) dari controller
                    var controllerActions = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                        .Where(method =>
                            method.IsPublic &&
                            !method.IsSpecialName &&
                            !method.GetCustomAttributes(typeof(NonActionAttribute), false).Any() &&
                            (method.GetCustomAttributes(typeof(HttpGetAttribute), false).Any() ||
                             method.GetCustomAttributes(typeof(HttpPostAttribute), false).Any() ||
                             method.GetCustomAttributes(typeof(HttpPutAttribute), false).Any() ||
                             method.GetCustomAttributes(typeof(HttpDeleteAttribute), false).Any()))
                        .Select(method => method.Name)
                        .ToList();

                    // Periksa controller tertentu untuk pengecualian
                    if (controllerName != "Account" && controllerName != "Auth" && controllerName != "Dashboard" && controllerName != "Home")
                    {
                        foreach (var action in controllerActions)
                        {
                            // Format role dengan memasukkan area dan sub-area serta controller dan action
                            // Hasil: ManajemenKesehatan_Master_Agama_GetAgama
                            string roleName = $"{areaName}_{subAreaName}_{controllerName}_{action}";

                            // Pastikan subAreaName tidak null (tidak memasukkan null pada nama role)
                            if (subAreaName != null)
                            {
                                roleName = $"{areaName}_{subAreaName}_{controllerName}_{action}";
                            }
                            else
                            {
                                roleName = $"{areaName}_{controllerName}_{action}";
                            }

                            // Buat role baru
                            IdentityRole role = new IdentityRole
                            {
                                Name = roleName,  // Nama role baru (misalnya, "ManajemenKesehatan_Master_Agama_GetAgama")
                                ConcurrencyStamp = controllerName
                            };

                            var result = await _roleManager.CreateAsync(role);
                            if (!result.Succeeded)
                            {
                                foreach (var error in result.Errors)
                                {
                                    Console.WriteLine($"Error creating role {roleName}: {error.Description}");
                                }
                            }
                        }
                    }
                }
            }

            // Ambil semua role setelah proses selesai
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            return Ok(new { message = "Roles created successfully!", roles = allRoles });
        }

        [HttpPost("AddPositionToRole")]
        public async Task<IActionResult> CreateRolePosition(string iduser, List<string> RoleId)
        {
            ViewBag.Active = "Administrator";
            var dateNow = DateTimeOffset.Now;
            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            var getUser = _applicationDbContext.UserActives.Where(u => u.Email == User.Identity.Name).FirstOrDefault();

            var getPosition = _roleManager.Roles.Select(r => r.Name).ToList();

            if (getPosition == null)
            {
                TempData["WarningMessage"] = "Sorry, please select a Position first !!!";
                return RedirectToAction("Index"); // atau aksi lain sesuai kebutuhan                
            }
            else
            {
                var positionId = _applicationDbContext.RolePositions.Where(u => u.PositionId == getUser.UserActiveId.ToString()).FirstOrDefault();

                // Hapus 
                _applicationDbContext.RolePositions.Remove(positionId);
                var userRoles = _applicationDbContext.UserRoles.Where(ur => ur.UserId == iduser).ToList();
                if (userRoles.Any())
                {
                    _applicationDbContext.UserRoles.RemoveRange(userRoles);
                    _applicationDbContext.SaveChanges();  // Simpan perubahan ke database
                }
                // End Hapus 

                if (ModelState.IsValid)
                {
                    // Simpan ID Peran
                    //foreach (var roleId in RoleId)
                    //{
                    //    var groupRole = new GroupRole
                    //    {
                    //        DepartemenId = positionId,
                    //        RoleId = roleId, // Gunakan roleId dari loop
                    //        CreateDateTime = DateTime.Now,
                    //        CreateBy = new Guid(getUser.Id)
                    //    };

                    //    _groupRoleRepository.Tambah(groupRole);

                    //}
                }

                TempData["SuccessMessage"] = "Role successfully assigned to Position";
                return RedirectToAction("Index"); // atau aksi lain sesuai kebutuhan
            }
        }

        // End Created Role Ke Table
       
    }
}
