using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Services;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    public class NFCController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly NFCReaderService _nfcService;

        // Constructor untuk Dependency Injection
        public NFCController(RoleManager<IdentityRole> roleManager, ApplicationDbContext applicationDbContext, NFCReaderService nfcService)
        {
            _roleManager = roleManager;
            _applicationDbContext = applicationDbContext;
            _nfcService = nfcService;
        }
        [HttpGet("read")]
        public async Task<IActionResult> ReadNFC()
        {
            string uid = await _nfcService.ReadNFCAsync();
            return Ok(new { uid });
        }

        [HttpPost("write-hello-world")]
        public async Task<IActionResult> WriteHelloWorld()
        {
            var reader = _nfcService.GetCurrentReader();
            if (reader == null)
            {
                return BadRequest("❌ NFC Reader belum dimulai.");
            }

            var result = await _nfcService.WriteHelloWorldToAnySector(reader);
            return Ok(new { message = result });
        }
    }
}
