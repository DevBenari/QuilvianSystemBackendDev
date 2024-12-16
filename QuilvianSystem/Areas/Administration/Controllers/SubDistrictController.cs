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
    public class SubDistrictController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SubDistrictController
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
        public IActionResult GetSubDistrict()
        {

            var SubDistrict = _applicationDbContext.SubDistricts.ToList();
            if (SubDistrict == null || !SubDistrict.Any())
            {
                return NotFound(new { message = "Belum ada data  SubDistrict." });
            }
            return Ok(SubDistrict);
        }
        // Get a specific sub-district by ID
        [HttpGet("{id}")]
        public IActionResult GetSubDistrictById(Guid id)
        {
            var subDistrict = _applicationDbContext.SubDistricts
                .Include(sd => sd.Country)
                .Include(sd => sd.Province)
                .Include(sd => sd.City)
                .Include(sd => sd.District)
                .FirstOrDefault(sd => sd.SubDistrictId == id);

            if (subDistrict == null)
                return NotFound();

            return Ok(subDistrict);
        }

        // Create a new sub-district
        [HttpPost]
        public IActionResult CreateSubDistrict([FromBody] SubDistrict subDistrict)
        {
            if (subDistrict == null)
                return BadRequest("Sub-district data is invalid.");

            _applicationDbContext.SubDistricts.Add(subDistrict);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetSubDistrictById), new { id = subDistrict.SubDistrictId }, subDistrict);
        }

        // Update a sub-district
        [HttpPut("{id}")]
        public IActionResult UpdateSubDistrict(Guid id, [FromBody] SubDistrict updatedSubDistrict)
        {
            var existingSubDistrict = _applicationDbContext.SubDistricts.Find(id);
            if (existingSubDistrict == null)
                return NotFound();

            existingSubDistrict.KodeKelurahan = updatedSubDistrict.KodeKelurahan;
            existingSubDistrict.NamaKelurahan = updatedSubDistrict.NamaKelurahan;
            existingSubDistrict.CountryId = updatedSubDistrict.CountryId;
            existingSubDistrict.ProvinceId = updatedSubDistrict.ProvinceId;
            existingSubDistrict.CityId = updatedSubDistrict.CityId;
            existingSubDistrict.DistrictId = updatedSubDistrict.DistrictId;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a sub-district
        [HttpDelete("{id}")]
        public IActionResult DeleteSubDistrict(Guid id)
        {
            var subDistrict = _applicationDbContext.SubDistricts.Find(id);
            if (subDistrict == null)
                return NotFound();

            _applicationDbContext.SubDistricts.Remove(subDistrict);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
   
}
