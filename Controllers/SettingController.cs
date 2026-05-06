using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/setting
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var setting = await _context.Settings.FirstOrDefaultAsync();

            if (setting == null)
                return NotFound("Setting not initialized");

            return Ok(setting);
        }
        public class SettingRepo
        {
            public Guid SettingId { get; set; }
            public string BaseUrlAi { get; set; } = string.Empty;
            public string ApiKeyAi { get; set; } = string.Empty;
            public string ModelAi { get; set; } = string.Empty;
            public string Prompt { get; set; } = string.Empty;
            public bool StatusAi { get; set; } = false;
        }
        // PUT: api/setting
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] SettingRepo setting)
        {
            // Ambil user login
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var getUserActive = await _context.UserActives
                .FirstOrDefaultAsync(u => u.Email == emailLogin);

            if (getUserActive == null)
                return Unauthorized("User tidak ditemukan");

            // Ambil data setting dari DB
            var data = await _context.Settings.FirstOrDefaultAsync();
            if (data == null)
                return NotFound("Setting belum diinisialisasi");

            // Hanya update field yang dikirim (tidak null)
            if (setting.BaseUrlAi != null) data.BaseUrlAi = setting.BaseUrlAi;
            if (setting.ApiKeyAi != null) data.ApiKeyAi = setting.ApiKeyAi;
            if (setting.ModelAi != null) data.ModelAi = setting.ModelAi;
            if (setting.Prompt != null) data.Prompt = setting.Prompt;
            if (setting.StatusAi) data.StatusAi = setting.StatusAi;

            data.UpdateBy = getUserActive.UserActiveId;
            data.UpdateDateTime = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(data);
        }


        // PATCH: api/setting/status
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] bool status)
        {
            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var getUserActive = await _context.UserActives
                .FirstOrDefaultAsync(u => u.Email == EmailLogin);

            var data = await _context.Settings.FirstOrDefaultAsync();

            if (data == null)
                return NotFound("Setting not initialized");

            data.StatusAi = status;
            data.UpdateBy = getUserActive.UserActiveId;
            data.UpdateDateTime = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(data);
        }
    }
}
