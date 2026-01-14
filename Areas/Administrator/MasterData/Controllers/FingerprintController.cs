using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.ViewModels;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class FingerprintController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<FingerprintController> _logger;

        public FingerprintController(ApplicationDbContext db, ILogger<FingerprintController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ===========================
        // 0. GET ALL FINGERPRINTS
        // ===========================
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFingerprints()
        {
            try
            {
                var list = await (from f in _db.Fingerprints
                                  join u in _db.UserActives
                                  on f.UserId equals u.UserActiveId.ToString() into gj
                                  from subu in gj.DefaultIfEmpty() // left join
                                  select new
                                  {
                                      f.FingerprintId,
                                      f.DeviceId,
                                      f.UserId,
                                      FullName = subu != null ? subu.FullName : null,
                                      f.Template,
                                      f.Status,
                                      f.CreateDateTime
                                  }).ToListAsync();

                var total = list.Count;


                if (list == null || list.Count == 0)
                    return NotFound(new { message = "Tidak ada data fingerprint di database" });

                return Ok(new
                {
                    message = "Berhasil mengambil semua data fingerprint",
                    total = list.Count,
                    data = list
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all fingerprints");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ===========================
        // 1. REGISTER FINGERPRINT
        // ===========================

        [HttpPost("register")]
        public async Task<IActionResult> RegisterFingerprint([FromBody] FingerprintRegisterViewModel vm)
        {
            if (vm == null || string.IsNullOrEmpty(vm.Template))
                return BadRequest(new { message = "Template fingerprint tidak valid" });

            try
            {                
                    var newData = new Fingerprint
                    {
                        FingerprintId = Guid.NewGuid(),
                        UserId = vm.UserId,
                        Template = vm.Template,
                        DeviceId = "Akun Baru",  // atau default lain
                        Status = "Active",             // atau default lain
                        CreateDateTime = DateTime.UtcNow
                    };

                    await _db.Fingerprints.AddAsync(newData);

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Fingerprint berhasil disimpan",
                    userId = vm.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering fingerprint");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("device-id/{fingerprintId}")]
        public async Task<IActionResult> UpdateDeviceIdByFingerprintId(
        Guid fingerprintId,
        [FromBody] FingerprintVerifyViewModel request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.DeviceId))
                    return BadRequest(new { message = "DeviceId wajib diisi" });

                var fingerprint = await _db.Fingerprints
                    .FirstOrDefaultAsync(x => x.FingerprintId == fingerprintId);

                if (fingerprint == null)
                    return NotFound(new
                    {
                        message = $"Fingerprint dengan FingerprintId '{fingerprintId}' tidak ditemukan"
                    });

                fingerprint.DeviceId = request.DeviceId;
                // fingerprint.UpdateDateTime = DateTime.Now; // jika ada

                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Berhasil mengupdate DeviceId fingerprint",
                    fingerprintId = fingerprintId,
                    deviceIdBaru = request.DeviceId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device id by fingerprint id");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{fingerprintId}")]
        public async Task<IActionResult> DeleteFingerprint(Guid fingerprintId)
        {
            try
            {
                var data = await _db.Fingerprints
                    .FirstOrDefaultAsync(x => x.FingerprintId == fingerprintId);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = $"Fingerprint dengan FingerprintId '{fingerprintId}' tidak ditemukan"
                    });
                }

                _db.Fingerprints.Remove(data);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Fingerprint berhasil dihapus",
                    fingerprintId = fingerprintId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fingerprint");
                return StatusCode(500, new { message = ex.Message });
            }
        }


        // ===========================
        // 4. DELETE FINGERPRINT BY USER ID
        // ===========================
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteFingerprint(string userId)
        {
            try
            {
                var data = await _db.Fingerprints
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (data == null)
                {
                    return NotFound(new
                    {
                        message = $"Fingerprint dengan UserId '{userId}' tidak ditemukan"
                    });
                }

                _db.Fingerprints.Remove(data);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    message = "Fingerprint berhasil dihapus",
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fingerprint");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
