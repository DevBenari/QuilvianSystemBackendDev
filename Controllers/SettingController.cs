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

        // PUT: api/setting
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Setting setting)
        {
            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var getUserActive = await _context.UserActives
                .FirstOrDefaultAsync(u => u.Email == EmailLogin);

            var data = await _context.Settings.FirstOrDefaultAsync();

            if (data == null)
                return NotFound("Setting not initialized");

            data.BaseUrlAi = setting.BaseUrlAi;
            data.ApiKeyAi = setting.ApiKeyAi;
            data.ModelAi = setting.ModelAi;
            data.StatusAi = setting.StatusAi;

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
