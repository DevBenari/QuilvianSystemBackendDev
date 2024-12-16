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
    public class LastEducationController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LastEducationController
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


        // Create a new last education entry
        [HttpPost("last-education")]
        public IActionResult CreateLastEducation([FromBody] LastEducation lastEducation)
        {
            if (lastEducation == null)
                return BadRequest("Invalid last education data.");

            _applicationDbContext.LastEducations.Add(lastEducation);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetLastEducation), new { id = lastEducation.LastEducationId }, lastEducation);
        }

        // Get all last education records
        [HttpGet("last-education")]
        public IActionResult GetAllLastEducations()
        {
            var lastEducations = _applicationDbContext.LastEducations.ToList();
            return Ok(lastEducations);
        }

        // Get a specific last education record by ID
        [HttpGet("last-education/{id}")]
        public IActionResult GetLastEducation(Guid id)
        {
            var lastEducation = _applicationDbContext.LastEducations.Find(id);
            if (lastEducation == null)
                return NotFound();

            return Ok(lastEducation);
        }

        // Update a last education record
        [HttpPut("last-education/{id}")]
        public IActionResult UpdateLastEducation(Guid id, [FromBody] LastEducation updatedLastEducation)
        {
            var existingLastEducation = _applicationDbContext.LastEducations.Find(id);
            if (existingLastEducation == null)
                return NotFound();

            existingLastEducation.KodePendidikanTerakhir = updatedLastEducation.KodePendidikanTerakhir;
            existingLastEducation.NamaPendidikanTerakhir = updatedLastEducation.NamaPendidikanTerakhir;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a last education record
        [HttpDelete("last-education/{id}")]
        public IActionResult DeleteLastEducation(Guid id)
        {
            var lastEducation = _applicationDbContext.LastEducations.Find(id);
            if (lastEducation == null)
                return NotFound();

            _applicationDbContext.LastEducations.Remove(lastEducation);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
   
}
