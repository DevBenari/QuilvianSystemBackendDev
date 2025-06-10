using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuilvianSystemBackendDev.Repositories;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    //[EnableCors("AllowSpecific")]
    public class RoleUserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _applicationDbContext;

        public RoleUserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet("GetAllRolesForUsers")]
        public async Task<IActionResult> GetAllRolesForUsers()
        {
            // Mengambil semua UserRoles yang terkait dengan setiap UserId
            var allUserRoles = await _applicationDbContext.UserRoles
                .GroupBy(ur => ur.UserId) // Mengelompokkan berdasarkan UserId
                .Select(group => new
                {
                    UserId = group.Key,
                    Roles = group.Select(ur => new
                    {
                        ur.RoleId,
                        RoleName = _applicationDbContext.Roles
                            .Where(role => role.Id == ur.RoleId)
                            .Select(role => role.Name) // Ambil nama role
                            .FirstOrDefault()
                    }).ToList()
                })
                .ToListAsync(); // Ambil semua grup berdasarkan UserId

            // Jika tidak ada data UserRoles ditemukan
            if (!allUserRoles.Any())
            {
                return NotFound(new { message = "Tidak ada data role yang ditemukan untuk user." });
            }

            return Ok(new
            {
                message = "Berhasil mengambil role untuk semua user.",
                data = allUserRoles
            });
        }

        // Method untuk menambahkan role ke user berdasarkan PositionId
        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole(Guid positionId, Guid pendaftaranPasienBaruId)
        {
            var newRolePositions = new List<IdentityUserRole<string>>();
            
            var existingRoles = _applicationDbContext.UserRoles
            .Where(ur => ur.UserId == pendaftaranPasienBaruId.ToString())
            .ToList();

            // Ambil RoleId yang terkait dengan PositionId dari RolePositions
            var rolePosition = await _applicationDbContext.RolePositions
                .Where(r => r.PositionId == positionId.ToString())
                .Select(r => r.RoleId)
                .ToListAsync(); // Ambil semua RoleId yang terkait dengan PositionId

            // Periksa setiap RoleId dan masukkan ke IdentityUserRole jika belum ada
            foreach (var roleId in rolePosition)
            {
                // Cek apakah role-position sudah ada di IdentityUserRole
                var exists = await _applicationDbContext.UserRoles
                    .AnyAsync(ur => ur.RoleId == roleId && ur.UserId == pendaftaranPasienBaruId.ToString());

                if (!exists)
                {
                    // Jika belum ada, tambahkan role ke IdentityUserRole
                    newRolePositions.Add(new IdentityUserRole<string>
                    {
                        UserId = pendaftaranPasienBaruId.ToString(), // User yang terdaftar (gunakan string karena IdentityUserRole menggunakan string)
                        RoleId = roleId.ToString() // Role yang terkait dengan PositionId
                    });
                }
            }

            // Menambahkan data ke IdentityUserRole
            if (newRolePositions.Any())
            {
                _applicationDbContext.UserRoles.AddRangeAsync(newRolePositions);
                await _applicationDbContext.SaveChangesAsync();
                return Ok(new { message = "Role untuk posisi berhasil ditambahkan ke user." });
            }

            return BadRequest(new { message = "Tidak ada role baru yang perlu ditambahkan." });
        }
    }
}
