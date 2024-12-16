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
    public class WorkingController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WorkingController
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


        // Create a new working entry
        [HttpPost("working")]
        public IActionResult CreateWorking([FromBody] Working working)
        {
            if (working == null)
                return BadRequest("Invalid working data.");

            _applicationDbContext.Workings.Add(working);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetWorking), new { id = working.WorkingId }, working);
        }

        // Get all working records
        [HttpGet("working")]
        public IActionResult GetAllWorkings()
        {
            var workings = _applicationDbContext.Workings.ToList();
            return Ok(workings);
        }

        // Get a specific working record by ID
        [HttpGet("working/{id}")]
        public IActionResult GetWorking(Guid id)
        {
            var working = _applicationDbContext.Workings.Find(id);
            if (working == null)
                return NotFound();

            return Ok(working);
        }

        // Update a working record
        [HttpPut("working/{id}")]
        public IActionResult UpdateWorking(Guid id, [FromBody] Working updatedWorking)
        {
            var existingWorking = _applicationDbContext.Workings.Find(id);
            if (existingWorking == null)
                return NotFound();

            existingWorking.KodePekerjaan = updatedWorking.KodePekerjaan;
            existingWorking.NamaPekerjaan = updatedWorking.NamaPekerjaan;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a working record
        [HttpDelete("working/{id}")]
        public IActionResult DeleteWorking(Guid id)
        {
            var working = _applicationDbContext.Workings.Find(id);
            if (working == null)
                return NotFound();

            _applicationDbContext.Workings.Remove(working);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }   
}
