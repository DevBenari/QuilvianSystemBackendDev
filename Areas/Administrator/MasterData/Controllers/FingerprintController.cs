using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.ViewModels;
using System.Collections.Concurrent;
using System.Linq;

namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FingerprintController : ControllerBase
    {
        // Opsional: in-memory storage sementara untuk testing
        private static readonly ConcurrentDictionary<string, Fingerprint> _tempStorage = new();

        // 1. VB kirim fingerprint ke TEMPORARY
        [HttpPost("temporary")]
        public IActionResult SaveTemporary([FromBody] FingerprintRegisterViewModel vm)
        {
            if (vm == null || string.IsNullOrEmpty(vm.UserId) || string.IsNullOrEmpty(vm.Template))
                return BadRequest(new { message = "UserId atau Template tidak valid" });

            var fingerprint = new Fingerprint
            {
                FingerprintId = Guid.NewGuid(),
                UserId = vm.UserId,
                DeviceId = $"DEV-{vm.UserId}",
                Template = vm.Template,
                Status = "Temporary"
            };

            _tempStorage[fingerprint.DeviceId] = fingerprint;

            return Ok(new { message = "Fingerprint disimpan sementara", fingerprint.DeviceId, fingerprint.UserId });
        }

        // 2. FE melakukan pendaftaran PERMANEN (misalnya ke DB)
        [HttpPost("register")]
        public IActionResult RegisterPermanent([FromBody] FingerprintRegisterViewModel vm)
        {
            if (vm == null || string.IsNullOrEmpty(vm.UserId))
                return BadRequest(new { message = "UserId tidak valid" });

            var deviceId = $"DEV-{vm.UserId}";

            if (!_tempStorage.TryGetValue(deviceId, out var tempFingerprint))
                return BadRequest(new { message = "Data fingerprint sementara tidak ditemukan" });

            // TODO: simpan tempFingerprint ke database permanen di sini
            // _dbContext.Fingerprints.Add(tempFingerprint);
            // _dbContext.SaveChanges();

            // Setelah sukses, hapus dari temporary
            _tempStorage.TryRemove(deviceId, out _);

            return Ok(new { message = "Fingerprint berhasil didaftarkan secara permanen", vm.UserId, deviceId });
        }


        // ===========================
        // 2. VERIFY FINGERPRINT (FE polling)
        // ===========================
        [HttpPost("verify")]
        public IActionResult Verify([FromBody] FingerprintVerifyViewModel vm)
        {
            if (vm == null || string.IsNullOrEmpty(vm.DeviceId))
                return BadRequest(new { message = "DeviceId tidak valid" });

            if (_tempStorage.TryGetValue(vm.DeviceId, out var stored))
            {
                bool isMatch = stored.Template == vm.Template;
                return Ok(new { match = isMatch });
            }

            return Ok(new { match = false });
        }

        // ===========================
        // 3. GET STATUS FINGERPRINT (FE polling)
        // ===========================
        [HttpGet("status/{deviceId}")]
        public IActionResult GetStatus(string deviceId)
        {
            if (_tempStorage.TryGetValue(deviceId, out var stored))
            {
                return Ok(new { match = true, template = stored.Template, userId = stored.UserId });
            }

            return Ok(new { match = false });
        }

        // ===========================
        // 3a. GET ALL TEMPORARY DATA
        // ===========================
        [HttpGet("temporary")]
        public IActionResult GetTemporaryData()
        {
            var data = _tempStorage.Values
                .Select(f => new
                {
                    f.FingerprintId,
                    f.UserId,
                    f.DeviceId,
                    f.Template,
                    f.Status
                })
                .ToList();

            return Ok(data);
        }

        // ===========================
        // 4. DELETE DATA SEMENTARA
        // ===========================
        [HttpDelete("{deviceId}")]
        public IActionResult Delete(string deviceId)
        {
            _tempStorage.TryRemove(deviceId, out _);
            return Ok(new { success = true, message = "Data fingerprint sementara dihapus" });
        }
    }
}


//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Cors;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
//using QuilvianSystemBackendDev.Areas.Administrator.MasterData.ViewModels;
//using QuilvianSystemBackendDev.Repositories;

//namespace QuilvianSystemBackendDev.Areas.Administrator.MasterData.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    //[Authorize]
//    //[EnableCors("AllowSpecific")]
//    public class FingerprintController : Controller
//    {
//        private readonly ApplicationDbContext _db;
//        private readonly ILogger<FingerprintController> _logger;

//        public FingerprintController(ApplicationDbContext db, ILogger<FingerprintController> logger)
//        {
//            _db = db;
//            _logger = logger;
//        }

//        // ===========================
//        // 0. GET ALL FINGERPRINTS
//        // ===========================
//        [HttpGet("all")]
//        public async Task<IActionResult> GetAllFingerprints()
//        {
//            try
//            {
//                var list = await _db.Fingerprints
//                    .Select(x => new
//                    {
//                        x.FingerprintId,
//                        x.DeviceId,
//                        x.UserId,
//                        x.Template,
//                        x.CreateDateTime
//                    })
//                    .ToListAsync();

//                if (list == null || list.Count == 0)
//                    return NotFound(new { message = "Tidak ada data fingerprint di database" });

//                return Ok(new
//                {
//                    message = "Berhasil mengambil semua data fingerprint",
//                    total = list.Count,
//                    data = list
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting all fingerprints");
//                return StatusCode(500, new { message = ex.Message });
//            }
//        }

//        // ===========================
//        // 1. REGISTER FINGERPRINT
//        // ===========================

//        [HttpPost("register")]
//        public async Task<IActionResult> RegisterFingerprint([FromBody] FingerprintRegisterViewModel vm)
//        {
//            if (vm == null || string.IsNullOrEmpty(vm.Template))
//                return BadRequest(new { message = "Template fingerprint tidak valid" });

//            try
//            {
//                var existing = await _db.Fingerprints
//                    .FirstOrDefaultAsync(x => x.UserId == vm.UserId);

//                if (existing != null)
//                {
//                    existing.Template = vm.Template;
//                    existing.CreateDateTime = DateTime.UtcNow;
//                    _db.Fingerprints.Update(existing);
//                }
//                else
//                {
//                    var newData = new Fingerprint
//                    {
//                        FingerprintId = Guid.NewGuid(),
//                        UserId = vm.UserId,
//                        Template = vm.Template,
//                        CreateDateTime = DateTime.UtcNow
//                    };

//                    await _db.Fingerprints.AddAsync(newData);
//                }

//                await _db.SaveChangesAsync();

//                return Ok(new
//                {
//                    message = "Fingerprint berhasil disimpan",
//                    userId = vm.UserId
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error registering fingerprint");
//                return StatusCode(500, new { message = ex.Message });
//            }
//        }

//        // ===========================
//        // 2. VERIFY FINGERPRINT
//        // ===========================
//        [HttpPost("verify")]
//        public async Task<IActionResult> VerifyFingerprint([FromBody] FingerprintVerifyViewModel vm)
//        {
//            if (vm == null || string.IsNullOrEmpty(vm.Template))
//                return BadRequest(new { message = "Template fingerprint tidak valid" });

//            try
//            {
//                var userFinger = await _db.Fingerprints
//                    .FirstOrDefaultAsync(x => x.UserId == vm.UserId);

//                if (userFinger == null)
//                    return NotFound(new { message = "Fingerprint user belum terdaftar" });

//                // ⭐⭐⭐
//                // DI SINI PROSES MATCHING TEMPLATE
//                // Pada server Linux, template hanya dibandingkan secara string
//                // Bila butuh real matching, harus dilakukan di Windows
//                // ⭐⭐⭐

//                bool isMatch = userFinger.Template == vm.Template;

//                return Ok(new
//                {
//                    message = "Fingerprint verifikasi berhasil",
//                    match = isMatch
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error verifying fingerprint");
//                return StatusCode(500, new { message = ex.Message });
//            }
//        }

//        // ===========================
//        // 3. GET DATA FINGERPRINT USER
//        // ===========================
//        [HttpGet("{DeviceId}")]
//        public async Task<IActionResult> GetFingerprint(string userId)
//        {
//            var data = await _db.Fingerprints.FirstOrDefaultAsync(x => x.UserId == userId);

//            if (data == null)
//                return NotFound(new { message = "Fingerprint tidak ditemukan" });

//            return Ok(new
//            {
//                message = "Fingerprint ditemukan",
//                data
//            });
//        }

//        // ===========================
//        // 4. DELETE FINGERPRINT BY USER ID
//        // ===========================
//        [HttpDelete("{userId}")]
//        public async Task<IActionResult> DeleteFingerprint(string userId)
//        {
//            try
//            {
//                var data = await _db.Fingerprints
//                    .FirstOrDefaultAsync(x => x.UserId == userId);

//                if (data == null)
//                {
//                    return NotFound(new
//                    {
//                        message = $"Fingerprint dengan UserId '{userId}' tidak ditemukan"
//                    });
//                }

//                _db.Fingerprints.Remove(data);
//                await _db.SaveChangesAsync();

//                return Ok(new
//                {
//                    message = "Fingerprint berhasil dihapus",
//                    userId = userId
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error deleting fingerprint");
//                return StatusCode(500, new { message = ex.Message });
//            }
//        }

//    }
//}
