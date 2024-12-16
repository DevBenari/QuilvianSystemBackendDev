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
    public class ReligionController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReligionController
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


        // Create a new religion entry
        [HttpPost("religion")]
        public IActionResult CreateReligion([FromBody] Religion religion)
        {
            if (religion == null)
                return BadRequest("Invalid religion data.");

            _applicationDbContext.Religions.Add(religion);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetReligion), new { id = religion.ReligionId }, religion);
        }

        // Get all religion records
        [HttpGet("religion")]
        public IActionResult GetAllReligions()
        {
            var religions = _applicationDbContext.Religions.ToList();
            return Ok(religions);
        }

        // Get a specific religion record by ID
        [HttpGet("religion/{id}")]
        public IActionResult GetReligion(Guid id)
        {
            var religion = _applicationDbContext.Religions.Find(id);
            if (religion == null)
                return NotFound();

            return Ok(religion);
        }

        // Update a religion record
        [HttpPut("religion/{id}")]
        public IActionResult UpdateReligion(Guid id, [FromBody] Religion updatedReligion)
        {
            var existingReligion = _applicationDbContext.Religions.Find(id);
            if (existingReligion == null)
                return NotFound();

            existingReligion.KodeAgama = updatedReligion.KodeAgama;
            existingReligion.NamaAgama = updatedReligion.NamaAgama;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a religion record
        [HttpDelete("religion/{id}")]
        public IActionResult DeleteReligion(Guid id)
        {
            var religion = _applicationDbContext.Religions.Find(id);
            if (religion == null)
                return NotFound();

            _applicationDbContext.Religions.Remove(religion);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
  
}
