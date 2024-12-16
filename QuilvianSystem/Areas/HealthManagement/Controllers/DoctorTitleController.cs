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
    public class DoctorTitleController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DoctorTitleController
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


        // Create a new doctor title
        [HttpPost("doctor-title")]
        public IActionResult CreateDoctorTitle([FromBody] DoctorTitle doctorTitle)
        {
            if (doctorTitle == null)
                return BadRequest("Invalid input");

            _applicationDbContext.DoctorTitles.Add(doctorTitle);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetDoctorTitle), new { id = doctorTitle.DoctorTitleId }, doctorTitle);
        }

        // Get all doctor titles
        [HttpGet("doctor-title")]
        public IActionResult GetAllDoctorTitles()
        {
            var titles = _applicationDbContext.DoctorTitles.ToList();
            return Ok(titles);
        }

        // Get a specific doctor title by ID
        [HttpGet("doctor-title/{id}")]
        public IActionResult GetDoctorTitle(Guid id)
        {
            var title = _applicationDbContext.DoctorTitles.Find(id);
            if (title == null)
                return NotFound();

            return Ok(title);
        }

        // Update a doctor title
        [HttpPut("doctor-title/{id}")]
        public IActionResult UpdateDoctorTitle(Guid id, [FromBody] DoctorTitle updatedDoctorTitle)
        {
            var existingTitle = _applicationDbContext.DoctorTitles.Find(id);
            if (existingTitle == null)
                return NotFound();

            // Update the properties
            existingTitle.KodeGelar = updatedDoctorTitle.KodeGelar;
            existingTitle.NamaGelar = updatedDoctorTitle.NamaGelar;
            existingTitle.Deskripsi = updatedDoctorTitle.Deskripsi;
            existingTitle.LapRL1 = updatedDoctorTitle.LapRL1;
            existingTitle.LapRL2 = updatedDoctorTitle.LapRL2;
            existingTitle.Status = updatedDoctorTitle.Status;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a doctor title
        [HttpDelete("doctor-title/{id}")]
        public IActionResult DeleteDoctorTitle(Guid id)
        {
            var title = _applicationDbContext.DoctorTitles.Find(id);
            if (title == null)
                return NotFound();

            _applicationDbContext.DoctorTitles.Remove(title);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
