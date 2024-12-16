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
    public class DoctorTypeController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DoctorTypeController
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

        // Create a new doctor type
        [HttpPost("doctor-type")]
        public IActionResult CreateDoctorType([FromBody] DoctorType doctorType)
        {
            if (doctorType == null)
                return BadRequest("Invalid input");

            _applicationDbContext.DoctorTypes.Add(doctorType);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetDoctorType), new { id = doctorType.DoctorTypeId }, doctorType);
        }

        // Get all doctor types
        [HttpGet("doctor-type")]
        public IActionResult GetAllDoctorTypes()
        {
            var doctorTypes = _applicationDbContext.DoctorTypes.ToList();
            return Ok(doctorTypes);
        }

        // Get a specific doctor type by ID
        [HttpGet("doctor-type/{id}")]
        public IActionResult GetDoctorType(Guid id)
        {
            var doctorType = _applicationDbContext.DoctorTypes.Find(id);
            if (doctorType == null)
                return NotFound();

            return Ok(doctorType);
        }

        // Update a doctor type
        [HttpPut("doctor-type/{id}")]
        public IActionResult UpdateDoctorType(Guid id, [FromBody] DoctorType updatedDoctorType)
        {
            var existingDoctorType = _applicationDbContext.DoctorTypes.Find(id);
            if (existingDoctorType == null)
                return NotFound();

            // Update the properties
            existingDoctorType.KodeTipeDokter = updatedDoctorType.KodeTipeDokter;
            existingDoctorType.TipeDokter = updatedDoctorType.TipeDokter;
            existingDoctorType.Persentase = updatedDoctorType.Persentase;
            existingDoctorType.Status = updatedDoctorType.Status;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a doctor type
        [HttpDelete("doctor-type/{id}")]
        public IActionResult DeleteDoctorType(Guid id)
        {
            var doctorType = _applicationDbContext.DoctorTypes.Find(id);
            if (doctorType == null)
                return NotFound();

            _applicationDbContext.DoctorTypes.Remove(doctorType);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
