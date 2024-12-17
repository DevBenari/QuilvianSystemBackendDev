using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystem.Areas.AccountingAndFinancial.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;

namespace QuilvianSystem.Areas.AccountingAndFinancial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    [EnableCors("AllowSpecific")]
    public class PendaftaranPasienController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PendaftaranPasienController
        (
                ApplicationDbContext applicationDbContext,
                UserManager<ApplicationUser> userManager,
                IWebHostEnvironment webHostEnvironment,
                SignInManager<ApplicationUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationDbContext = applicationDbContext;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: api/PendaftaranPasien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PendaftaranPasien>>> GetPendaftaranPasien()
        {
            return await _applicationDbContext.PendaftaranPasiens.ToListAsync();
        }

        // GET: api/PendaftaranPasien/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PendaftaranPasien>> GetPendaftaranPasien(Guid id)
        {
            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasiens.FindAsync(id);

            if (pendaftaranPasien == null)
            {
                return NotFound();
            }

            return pendaftaranPasien;
        }

        // POST: api/PendaftaranPasien
        [HttpPost]
        public async Task<ActionResult<PendaftaranPasien>> PostPendaftaranPasien(PendaftaranPasien pendaftaranPasien)
        {
            pendaftaranPasien.PendaftaranPasienId = Guid.NewGuid(); // Generate a new GUID for the ID
            _applicationDbContext.PendaftaranPasiens.Add(pendaftaranPasien);
            await _applicationDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPendaftaranPasien), new { id = pendaftaranPasien.PendaftaranPasienId }, pendaftaranPasien);
        }

        // PUT: api/PendaftaranPasien/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPendaftaranPasien(Guid id, PendaftaranPasien pendaftaranPasien)
        {
            if (id != pendaftaranPasien.PendaftaranPasienId)
            {
                return BadRequest();
            }

            _applicationDbContext.Entry(pendaftaranPasien).State = EntityState.Modified;

            try
            {
                await _applicationDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PendaftaranPasienExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/PendaftaranPasien/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePendaftaranPasien(Guid id)
        {
            var pendaftaranPasien = await _applicationDbContext.PendaftaranPasiens.FindAsync(id);
            if (pendaftaranPasien == null)
            {
                return NotFound();
            }

            _applicationDbContext.PendaftaranPasiens.Remove(pendaftaranPasien);
            await _applicationDbContext.SaveChangesAsync();

            return NoContent();
        }

        private bool PendaftaranPasienExists(Guid id)
        {
            return _applicationDbContext.PendaftaranPasiens.Any(e => e.PendaftaranPasienId == id);
        }
    }
}
