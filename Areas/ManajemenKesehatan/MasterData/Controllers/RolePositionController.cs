using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models; // Model untuk RolePosition
using QuilvianSystemBackendDev.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    //[EnableCors("AllowSpecific")]
    public class RolePositionController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public RolePositionController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        // Mengambil daftar role position
        [HttpGet]
        public async Task<IActionResult> LoadRolePositions()
        {
            try
            {
                var rolePositions = await _applicationDbContext.RolePositions
                    .Select(r => new
                    {
                        r.Id,
                        r.RoleId,
                        r.PositionId
                    })
                    .OrderBy(r => r.RoleId)
                    .ToListAsync();

                return Ok(rolePositions); // JSON result
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "❌ Server error: " + ex.Message });
            }
        }


        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var groupedRoles = await _applicationDbContext.Roles
                    .GroupBy(r => r.ConcurrencyStamp)
                    .Select(g => new
                    {
                        ConcurrencyStamp = g.Key,
                        Roles = g.Select(r => new
                        {
                            r.Id,
                            r.Name,
                            r.NormalizedName
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    totalGroups = groupedRoles.Count,
                    data = groupedRoles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "❌ Server error: " + ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("GenerateCrudRoles")]
        public async Task<IActionResult> GenerateCrudRoles(
        [FromServices] IActionDescriptorCollectionProvider actionProvider)
        {
            // 1. Hapus semua data di tabel AspNetRoles
            var allRoles = _applicationDbContext.Roles.ToList();
            _applicationDbContext.Roles.RemoveRange(allRoles);
            await _applicationDbContext.SaveChangesAsync();
            try
            {
                var actionDescriptors = actionProvider.ActionDescriptors.Items
                    .OfType<ControllerActionDescriptor>()
                    .Where(a => a.ControllerTypeInfo.IsPublic &&
                                !a.ControllerName.StartsWith("Identity", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var existingRoleNames = await _applicationDbContext.Roles
                    .Select(r => r.NormalizedName)
                    .ToListAsync();

                var newRoles = new List<IdentityRole>();

                foreach (var action in actionDescriptors)
                {
                    var httpMethod = action.EndpointMetadata
                        .OfType<HttpMethodAttribute>()
                        .FirstOrDefault()?.HttpMethods.FirstOrDefault() ?? "UNKNOWN";

                    // Format roleName: HttpMethod_ActionName
                    var roleName = $"{httpMethod}_{action.ActionName}";

                    // Batasi panjang maksimal 256 karakter
                    if (roleName.Length > 256)
                        roleName = roleName.Substring(0, 256);

                    var normalizedRoleName = roleName.ToUpper();

                    if (!existingRoleNames.Contains(normalizedRoleName))
                    {
                        using var md5 = System.Security.Cryptography.MD5.Create();
                        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(normalizedRoleName));
                        var roleId = new Guid(hash).ToString();

                        newRoles.Add(new IdentityRole
                        {
                            Id = roleId,
                            Name = roleName,
                            NormalizedName = normalizedRoleName,
                            ConcurrencyStamp = action.ControllerName
                        });

                        existingRoleNames.Add(normalizedRoleName);
                    }
                }

                if (newRoles.Count > 0)
                {
                    await _applicationDbContext.Roles.AddRangeAsync(newRoles);
                    await _applicationDbContext.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "✅ Role untuk semua controller & action berhasil ditambahkan.",
                    totalInserted = newRoles.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "❌ Server error: " + ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }


        // Menambahkan role position baru
        [HttpPost]
        public async Task<IActionResult> CreateRolePosition([FromBody] CreateRolePositionRequest request)
        {
            if (request.RoleId == null || !request.RoleId.Any() || string.IsNullOrEmpty(request.PositionId))
            {
                return BadRequest(new { success = false, message = "RoleId dan PositionId diperlukan." });
            }

            try
            {
                var newRolePositions = new List<RolePosition>();

                foreach (var roleId in request.RoleId)
                {
                    // Cek apakah role-position sudah ada
                    var exists = await _applicationDbContext.RolePositions
                        .AnyAsync(r => r.RoleId == roleId && r.PositionId == request.PositionId);

                    if (!exists)
                    {
                        newRolePositions.Add(new RolePosition
                        {
                            Id = Guid.NewGuid(),
                            RoleId = roleId,
                            PositionId = request.PositionId
                        });
                    }
                }

                if (!newRolePositions.Any())
                {
                    return BadRequest(new { success = false, message = "Semua kombinasi RoleId dan PositionId sudah ada." });
                }

                _applicationDbContext.RolePositions.AddRange(newRolePositions);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "RolePosition berhasil ditambahkan.", data = newRolePositions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "❌ Server error: " + ex.Message });
            }
        }

        // Mengambil data role position berdasarkan ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRolePositionById(Guid id)
        {
            try
            {
                var rolePosition = await _applicationDbContext.RolePositions
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rolePosition == null)
                {
                    return NotFound(new { success = false, message = "RolePosition tidak ditemukan." });
                }

                return Ok(new
                {
                    rolePosition.Id,
                    rolePosition.RoleId,
                    rolePosition.PositionId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "❌ Server error: " + ex.Message });
            }
        }

        // Menghapus role position berdasarkan ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRolePosition(Guid id)
        {
            try
            {
                var rolePosition = await _applicationDbContext.RolePositions
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rolePosition == null)
                {
                    return NotFound(new { success = false, message = "RolePosition tidak ditemukan." });
                }

                _applicationDbContext.RolePositions.Remove(rolePosition);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { success = true, message = "RolePosition berhasil dihapus." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "❌ Server error: " + ex.Message });
            }
        }
    }

    // DTO untuk menerima data RolePosition
    public class CreateRolePositionRequest
    {
        public List<string> RoleId { get; set; }
        public string PositionId { get; set; }
    }
}
