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
    public class ProvinceController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProvinceController
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
        public IActionResult GetProvince()
        {

            var Province = _applicationDbContext.Provinces.ToList();
            if (Province == null || !Province.Any())
            {
                return NotFound(new { message = "Belum ada data  Province." });
            }
            return Ok(Province);
        }

        // Get a specific province by ID
        [HttpGet("{id}")]
        public IActionResult GetProvinceById(Guid id)
        {
            var province = _applicationDbContext.Provinces
                .Include(p => p.Country)
                .FirstOrDefault(p => p.ProvinceId == id);

            if (province == null)
                return NotFound();

            return Ok(province);
        }

        // Create a new province
        [HttpPost]
        public IActionResult CreateProvince([FromBody] Province province)
        {
            if (province == null)
                return BadRequest("Invalid province data.");

            _applicationDbContext.Provinces.Add(province);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetProvinceById), new { id = province.ProvinceId }, province);
        }

        // Update a province
        [HttpPut("{id}")]
        public IActionResult UpdateProvince(Guid id, [FromBody] Province updatedProvince)
        {
            var existingProvince = _applicationDbContext.Provinces.Find(id);
            if (existingProvince == null)
                return NotFound();

            existingProvince.KodeProvinsi = updatedProvince.KodeProvinsi;
            existingProvince.NamaProvinsi = updatedProvince.NamaProvinsi;
            existingProvince.CountryId = updatedProvince.CountryId;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a province
        [HttpDelete("{id}")]
        public IActionResult DeleteProvince(Guid id)
        {
            var province = _applicationDbContext.Provinces.Find(id);
            if (province == null)
                return NotFound();

            _applicationDbContext.Provinces.Remove(province);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Get all provinces by country
        [HttpGet("by-country/{countryId}")]
        public IActionResult GetProvincesByCountry(Guid countryId)
        {
            var provinces = _applicationDbContext.Provinces
                .Where(p => p.CountryId == countryId)
                .Include(p => p.Country)
                .ToList();

            return Ok(provinces);
        }
    }
  
}
