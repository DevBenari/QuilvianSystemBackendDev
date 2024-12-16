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
    public class DoctorQueueTypeController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DoctorQueueTypeController
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


        // Create a new doctor queue type
        [HttpPost("doctor-queue-type")]
        public IActionResult CreateDoctorQueueType([FromBody] DoctorQueueType doctorQueueType)
        {
            if (doctorQueueType == null)
                return BadRequest("Invalid input");

            _applicationDbContext.DoctorQueueTypes.Add(doctorQueueType);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetDoctorQueueType), new { id = doctorQueueType.DoctorQueueTypeId }, doctorQueueType);
        }

        // Get all doctor queue types
        [HttpGet("doctor-queue-type")]
        public IActionResult GetAllDoctorQueueTypes()
        {
            var queueTypes = _applicationDbContext.DoctorQueueTypes.ToList();
            return Ok(queueTypes);
        }

        // Get a specific doctor queue type by ID
        [HttpGet("doctor-queue-type/{id}")]
        public IActionResult GetDoctorQueueType(Guid id)
        {
            var queueType = _applicationDbContext.DoctorQueueTypes.Find(id);
            if (queueType == null)
                return NotFound();

            return Ok(queueType);
        }

        // Update a doctor queue type
        [HttpPut("doctor-queue-type/{id}")]
        public IActionResult UpdateDoctorQueueType(Guid id, [FromBody] DoctorQueueType updatedQueueType)
        {
            var existingQueueType = _applicationDbContext.DoctorQueueTypes.Find(id);
            if (existingQueueType == null)
                return NotFound();

            // Update the properties
            existingQueueType.KodeTipeAntrian = updatedQueueType.KodeTipeAntrian;
            existingQueueType.NamaTipeAntrian = updatedQueueType.NamaTipeAntrian;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a doctor queue type
        [HttpDelete("doctor-queue-type/{id}")]
        public IActionResult DeleteDoctorQueueType(Guid id)
        {
            var queueType = _applicationDbContext.DoctorQueueTypes.Find(id);
            if (queueType == null)
                return NotFound();

            _applicationDbContext.DoctorQueueTypes.Remove(queueType);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
