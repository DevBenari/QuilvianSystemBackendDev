using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;
using System.Data.SqlClient;

namespace BenariMikronWebApp.Areas.Administration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class DistrictController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DistrictController
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

        [HttpGet]
        public IActionResult GetDistrict()
        {

            var District = _applicationDbContext.Countries.ToList();
            if (District == null || !District.Any())
            {
                return NotFound(new { message = "Belum ada data  District." });
            }
            return Ok(District);
        }

        // Get a specific district by ID
        [HttpGet("{id}")]
        public IActionResult GetDistrictById(Guid id)
        {
            var district = _applicationDbContext.Districts
                .Include(d => d.Country)
                .Include(d => d.Province)
                .Include(d => d.City)
                .FirstOrDefault(d => d.DistrictId == id);

            if (district == null)
                return NotFound();

            return Ok(district);
        }

        // Create a new district
        [HttpPost]
        public IActionResult CreateDistrict([FromBody] District district)
        {
            if (district == null)
                return BadRequest("District data is invalid.");

            _applicationDbContext.Districts.Add(district);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetDistrictById), new { id = district.DistrictId }, district);
        }

        // Update a district
        [HttpPut("{id}")]
        public IActionResult UpdateDistrict(Guid id, [FromBody] District updatedDistrict)
        {
            var existingDistrict = _applicationDbContext.Districts.Find(id);
            if (existingDistrict == null)
                return NotFound();

            existingDistrict.KodeKecamatan = updatedDistrict.KodeKecamatan;
            existingDistrict.NamaKecamatan = updatedDistrict.NamaKecamatan;
            existingDistrict.CountryId = updatedDistrict.CountryId;
            existingDistrict.ProvinceId = updatedDistrict.ProvinceId;
            existingDistrict.CityId = updatedDistrict.CityId;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a district
        [HttpDelete("{id}")]
        public IActionResult DeleteDistrict(Guid id)
        {
            var district = _applicationDbContext.Districts.Find(id);
            if (district == null)
                return NotFound();

            _applicationDbContext.Districts.Remove(district);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
