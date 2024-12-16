using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystem.Areas.HealthManagement.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;

namespace QuilvianSystem.Areas.HealthManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class BloodTypeController : Controller
    {

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BloodTypeController
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
        // Create a new blood type record
        [HttpPost("bloodtype")]
        public IActionResult CreateBloodType([FromBody] BloodType bloodType)
        {
            if (bloodType == null)
                return BadRequest("Invalid input");

            _applicationDbContext.BloodTypes.Add(bloodType);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetBloodType), new { id = bloodType.BloodTypeId }, bloodType);
        }

        // Get all blood type records
        [HttpGet("bloodtype")]
        public IActionResult GetAllBloodTypes()
        {
            var bloodTypes = _applicationDbContext.BloodTypes.ToList();
            return Ok(bloodTypes);
        }

        // Get a specific blood type record by ID
        [HttpGet("bloodtype/{id}")]
        public IActionResult GetBloodType(Guid id)
        {
            var bloodType = _applicationDbContext.BloodTypes.Find(id);
            if (bloodType == null)
                return NotFound();

            return Ok(bloodType);
        }

        // Update a blood type record
        [HttpPut("bloodtype/{id}")]
        public IActionResult UpdateBloodType(Guid id, [FromBody] BloodType updatedBloodType)
        {
            var existingBloodType = _applicationDbContext.BloodTypes.Find(id);
            if (existingBloodType == null)
                return NotFound();

            // Update fields
            existingBloodType.KodeGolonganDarah = updatedBloodType.KodeGolonganDarah;
            existingBloodType.NamaGolonganDarah = updatedBloodType.NamaGolonganDarah;
            existingBloodType.Keterangan = updatedBloodType.Keterangan;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a blood type record
        [HttpDelete("bloodtype/{id}")]
        public IActionResult DeleteBloodType(Guid id)
        {
            var bloodType = _applicationDbContext.BloodTypes.Find(id);
            if (bloodType == null)
                return NotFound();

            _applicationDbContext.BloodTypes.Remove(bloodType);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
