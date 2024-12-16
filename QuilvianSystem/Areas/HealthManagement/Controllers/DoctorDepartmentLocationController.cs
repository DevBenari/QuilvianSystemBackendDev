using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
    [EnableCors("AllowSpecific")]
    public class DepartmentLocationController : Controller
    {

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DepartmentLocationController
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


        // Create a new department location
        [HttpPost("department-location")]
        public IActionResult CreateDepartmentLocation([FromBody] DoctorDepartmentLocation departmentLocation)
        {
            if (departmentLocation == null)
                return BadRequest("Invalid input");

            _applicationDbContext.DepartmentLocations.Add(departmentLocation);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetDepartmentLocation), new { id = departmentLocation.LocationId }, departmentLocation);
        }

        // Get all department locations
        [HttpGet("department-location")]
        public IActionResult GetAllDepartmentLocations()
        {
            var locations = _applicationDbContext.DepartmentLocations.ToList();
            return Ok(locations);
        }

        // Get a specific department location by ID
        [HttpGet("department-location/{id}")]
        public IActionResult GetDepartmentLocation(Guid id)
        {
            var location = _applicationDbContext.DepartmentLocations.Find(id);
            if (location == null)
                return NotFound();

            return Ok(location);
        }

        // Update a department location
        [HttpPut("department-location/{id}")]
        public IActionResult UpdateDepartmentLocation(Guid id, [FromBody] DoctorDepartmentLocation updatedLocation)
        {
            var existingLocation = _applicationDbContext.DepartmentLocations.Find(id);
            if (existingLocation == null)
                return NotFound();

            // Update location fields
            existingLocation.KodeLokasi = updatedLocation.KodeLokasi;
            existingLocation.NamaLokasi = updatedLocation.NamaLokasi;
            existingLocation.Keterangan = updatedLocation.Keterangan;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a department location
        [HttpDelete("department-location/{id}")]
        public IActionResult DeleteDepartmentLocation(Guid id)
        {
            var location = _applicationDbContext.DepartmentLocations.Find(id);
            if (location == null)
                return NotFound();

            _applicationDbContext.DepartmentLocations.Remove(location);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
