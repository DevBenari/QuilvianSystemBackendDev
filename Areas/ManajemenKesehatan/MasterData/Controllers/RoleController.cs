using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    //[EnableCors("AllowSpecific")]
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public RoleController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _applicationDbContext.Roles
                    .Select(r => new
                    {
                        r.Id,
                        r.Name,
                        r.NormalizedName,
                        r.ConcurrencyStamp
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    total = roles.Count,
                    data = roles
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


        [HttpGet("GetAllRolesGroup")]
        public async Task<IActionResult> GetAllRolesGroup()
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

    }
}
