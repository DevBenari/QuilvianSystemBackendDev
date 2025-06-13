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
                var rolePositions = await (from rp in _applicationDbContext.RolePositions
                                           join role in _applicationDbContext.Roles on rp.RoleId equals role.Id
                                           join position in _applicationDbContext.Positions on rp.PositionId equals position.PositionId.ToString()
                                           orderby role.Id
                                           select new
                                           {
                                               rp.Id,
                                               rp.RoleId,
                                               RoleName = role.Name,
                                               rp.PositionId,
                                               PositionName = position.PositionName
                                           }).ToListAsync();

                return Ok(rolePositions); // JSON result
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "❌ Server error: " + ex.Message });
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
                var rolePositions = await _applicationDbContext.RolePositions
                    .Where(r => r.PositionId == id.ToString())  // Mengambil semua RolePosition yang memiliki PositionId sama
                    .ToListAsync();  // Mengambil hasil sebagai list secara asinkron

                if (rolePositions == null || !rolePositions.Any())
                {
                    return NotFound(new { success = false, message = "RolePosition tidak ditemukan." });
                }

                // Mengembalikan data yang lebih dari satu dengan properti yang diinginkan
                return Ok(rolePositions.Select(rp => new
                {
                    rp.Id,
                    rp.RoleId,
                    rp.PositionId
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "❌ Server error: " + ex.Message });
            }
        }

        // Get Group Role
        [HttpGet("bygroup/{idposition}")]
        public async Task<IActionResult> GetRolePositionByGroup(Guid idposition)
        {
            try
            {
                var roleData = await (
                    from rp in _applicationDbContext.RolePositions
                    join r in _applicationDbContext.Roles on rp.RoleId equals r.Id
                    where rp.PositionId == idposition.ToString()
                    select new
                    {
                        r.Id,
                        r.Name,
                        r.NormalizedName,
                        r.ConcurrencyStamp
                    }).ToListAsync();

                if (!roleData.Any())
                {
                    return NotFound(new { success = false, message = "RolePosition tidak ditemukan." });
                }

                var groupedData = roleData
                    .GroupBy(r => r.ConcurrencyStamp)
                    .Select(g => new
                    {
                        concurrencyStamp = g.Key,
                        roles = g.Select(r => new
                        {
                            id = r.Id,
                            name = r.Name,
                            normalizedName = r.NormalizedName
                        }).ToList()
                    }).ToList();

                return Ok(new
                {
                    success = true,
                    totalGroups = groupedData.Count,
                    data = groupedData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "❌ Server error: " + ex.Message });
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
