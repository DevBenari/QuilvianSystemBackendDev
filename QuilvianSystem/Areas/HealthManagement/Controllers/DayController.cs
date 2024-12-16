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
    public class DayController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DayController
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

        // Create a new day record
        [HttpPost("day")]
        public IActionResult CreateDay([FromBody] Day day)
        {
            if (day == null)
                return BadRequest("Invalid input");

            _applicationDbContext.Days.Add(day);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetDay), new { id = day.DayId }, day);
        }

        // Get all day records
        [HttpGet("day")]
        public IActionResult GetAllDays()
        {
            var days = _applicationDbContext.Days.ToList();
            return Ok(days);
        }

        // Get a specific day record by ID
        [HttpGet("day/{id}")]
        public IActionResult GetDay(Guid id)
        {
            var day = _applicationDbContext.Days.Find(id);
            if (day == null)
                return NotFound();

            return Ok(day);
        }

        // Update a day record
        [HttpPut("day/{id}")]
        public IActionResult UpdateDay(Guid id, [FromBody] Day updatedDay)
        {
            var existingDay = _applicationDbContext.Days.Find(id);
            if (existingDay == null)
                return NotFound();

            // Update fields
            existingDay.KodeHari = updatedDay.KodeHari;
            existingDay.NamaHari = updatedDay.NamaHari;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a day record
        [HttpDelete("day/{id}")]
        public IActionResult DeleteDay(Guid id)
        {
            var day = _applicationDbContext.Days.Find(id);
            if (day == null)
                return NotFound();

            _applicationDbContext.Days.Remove(day);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
