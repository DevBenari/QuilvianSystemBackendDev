using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;

namespace BenariMikronWebApp.Areas.Administration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class PromoController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PromoController
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

        // Create a new promo entry
        [HttpPost("promo")]
        public IActionResult CreatePromo([FromBody] Promo promo)
        {
            if (promo == null)
                return BadRequest("Invalid promo data.");

            _applicationDbContext.Promos.Add(promo);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetPromo), new { id = promo.PromoId }, promo);
        }

        // Get all promo records
        [HttpGet("promo")]
        public IActionResult GetAllPromos()
        {
            var promos = _applicationDbContext.Promos.ToList();
            return Ok(promos);
        }

        // Get a specific promo record by ID
        [HttpGet("promo/{id}")]
        public IActionResult GetPromo(Guid id)
        {
            var promo = _applicationDbContext.Promos.Find(id);
            if (promo == null)
                return NotFound();

            return Ok(promo);
        }

        // Update a promo record
        [HttpPut("promo/{id}")]
        public IActionResult UpdatePromo(Guid id, [FromBody] Promo updatedPromo)
        {
            var existingPromo = _applicationDbContext.Promos.Find(id);
            if (existingPromo == null)
                return NotFound();

            existingPromo.KodePromo = updatedPromo.KodePromo;
            existingPromo.NamaPromo = updatedPromo.NamaPromo;
            existingPromo.Keterangan = updatedPromo.Keterangan;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a promo record
        [HttpDelete("promo/{id}")]
        public IActionResult DeletePromo(Guid id)
        {
            var promo = _applicationDbContext.Promos.Find(id);
            if (promo == null)
                return NotFound();

            _applicationDbContext.Promos.Remove(promo);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
   
}
