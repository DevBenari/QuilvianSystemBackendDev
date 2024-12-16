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
    public class DoctorDepartmentController : Controller
    {

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DoctorDepartmentController
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

        // Get all doctor department records
        [HttpGet("doctor-department")]
        public IActionResult GetAllDoctorDepartments()
        {
            var departments = _applicationDbContext.DoctorDepartments.ToList();
            return Ok(departments);
        }

        // Create a new doctor department record
        [HttpPost("doctor-department")]
        public IActionResult CreateDoctorDepartment([FromBody] DoctorDepartment department)
        {
            if (department == null)
                return BadRequest("Invalid input");

            _applicationDbContext.DoctorDepartments.Add(department);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetDoctorDepartment), new { id = department.DepartmentId }, department);
        }

        // Get a specific doctor department record by ID
        [HttpGet("doctor-department/{id}")]
        public IActionResult GetDoctorDepartment(Guid id)
        {
            var department = _applicationDbContext.DoctorDepartments.Find(id);
            if (department == null)
                return NotFound();

            return Ok(department);
        }

        // Update a doctor department record
        [HttpPut("doctor-department/{id}")]
        public IActionResult UpdateDoctorDepartment(Guid id, [FromBody] DoctorDepartment updatedDepartment)
        {
            var existingDepartment = _applicationDbContext.DoctorDepartments.Find(id);
            if (existingDepartment == null)
                return NotFound();

            // Update department fields
            existingDepartment.KodeDepartemen = updatedDepartment.KodeDepartemen;
            existingDepartment.NamaDepartemen = updatedDepartment.NamaDepartemen;
            existingDepartment.LocationId = updatedDepartment.LocationId;
            existingDepartment.Telepon = updatedDepartment.Telepon;
            existingDepartment.MulaiJamKerja = updatedDepartment.MulaiJamKerja;
            existingDepartment.SelesaiJamKerja = updatedDepartment.SelesaiJamKerja;
            existingDepartment.Keterangan = updatedDepartment.Keterangan;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a doctor department record
        [HttpDelete("doctor-department/{id}")]
        public IActionResult DeleteDoctorDepartment(Guid id)
        {
            var department = _applicationDbContext.DoctorDepartments.Find(id);
            if (department == null)
                return NotFound();

            _applicationDbContext.DoctorDepartments.Remove(department);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }

    }
}
