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
    //[Authorize]
    //[EnableCors("AllowSpecific")]
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
        // 1. REGISTER FINGERPRINT
        // ===========================

        [HttpPost("register")]
        public async Task<IActionResult> RegisterFingerprint([FromBody] FingerprintRegisterViewModel vm)
        {
            if (vm == null || string.IsNullOrEmpty(vm.Template))
                return BadRequest(new { message = "Template fingerprint tidak valid" });

            try
            {
                var existing = await _db.Fingerprints
                    .FirstOrDefaultAsync(x => x.UserId == vm.UserId);

                if (existing != null)
                {
                    existing.Template = vm.Template;
                    existing.CreateDateTime = DateTime.UtcNow;
                    _db.Fingerprints.Update(existing);
                }
                else
                {
                    var newData = new Fingerprint
                    {
                        FingerprintId = Guid.NewGuid(),
                        UserId = vm.UserId,
                        Template = vm.Template,
                        CreateDateTime = DateTime.UtcNow
                    };

                    await _db.Fingerprints.AddAsync(newData);
                }

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

        // ===========================
        // 2. VERIFY FINGERPRINT
        // ===========================
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyFingerprint([FromBody] FingerprintVerifyViewModel vm)
        {
            if (vm == null || string.IsNullOrEmpty(vm.Template))
                return BadRequest(new { message = "Template fingerprint tidak valid" });

            try
            {
                var userFinger = await _db.Fingerprints
                    .FirstOrDefaultAsync(x => x.UserId == vm.UserId);

                if (userFinger == null)
                    return NotFound(new { message = "Fingerprint user belum terdaftar" });

                // ⭐⭐⭐
                // DI SINI PROSES MATCHING TEMPLATE
                // Pada server Linux, template hanya dibandingkan secara string
                // Bila butuh real matching, harus dilakukan di Windows
                // ⭐⭐⭐

                bool isMatch = userFinger.Template == vm.Template;

                return Ok(new
                {
                    message = "Fingerprint verifikasi berhasil",
                    match = isMatch
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying fingerprint");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ===========================
        // 3. GET DATA FINGERPRINT USER
        // ===========================
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFingerprint(string userId)
        {
            var data = await _db.Fingerprints.FirstOrDefaultAsync(x => x.UserId == userId);

            if (data == null)
                return NotFound(new { message = "Fingerprint tidak ditemukan" });

            return Ok(new
            {
                message = "Fingerprint ditemukan",
                data
            });
        }

    }
}
