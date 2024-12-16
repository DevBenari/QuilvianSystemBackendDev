using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Areas.MasterData.Models;
using QuilvianSystem.Areas.MasterData.ViewModels;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;
using System.Data;

namespace BenariMikronWebApp.Areas.Administration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class CityController : Controller
    {

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CityController
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
        public IActionResult GetCity()
        {

            var City = _applicationDbContext.Cities.ToList();
            if (City == null || !City.Any())
            {
                return NotFound(new { message = "Belum ada data City." });
            }
            return Ok(City);
        }

        [HttpPost]
        public async Task<IActionResult> AddCity([FromBody] City model)
        {
            if (model == null)
                return BadRequest("Invalid city data.");

            model.CityId = Guid.NewGuid(); // Generate new ID
            _applicationDbContext.Cities.Add(model);

            await _applicationDbContext.SaveChangesAsync();
            return Ok(new { message = "City added successfully", cityId = model.CityId });
        }
        // Get a specific city by ID
        [HttpGet("{id}")]
        public IActionResult GetCityById(Guid id)
        {
            var city = _applicationDbContext.Cities
                .Include(c => c.Province)
                .Include(c => c.Country)
                .FirstOrDefault(c => c.CityId == id);

            if (city == null)
                return NotFound();

            return Ok(city);
        }
        // Update a city
        [HttpPut("{id}")]
        public IActionResult UpdateCity(Guid id, [FromBody] City updatedCity)
        {
            var existingCity = _applicationDbContext.Cities.Find(id);
            if (existingCity == null)
                return NotFound();

            // Update fields
            existingCity.KodeKota = updatedCity.KodeKota;
            existingCity.NamaKota = updatedCity.NamaKota;
            existingCity.ProvinceId = updatedCity.ProvinceId;
            existingCity.CountryId = updatedCity.CountryId;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a city
        [HttpDelete("{id}")]
        public IActionResult DeleteCity(Guid id)
        {
            var city = _applicationDbContext.Cities.Find(id);
            if (city == null)
                return NotFound();

            _applicationDbContext.Cities.Remove(city);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Get all cities by province
        [HttpGet("by-province/{provinceId}")]
        public IActionResult GetCitiesByProvince(Guid provinceId)
        {
            var cities = _applicationDbContext.Cities
                .Where(c => c.ProvinceId == provinceId)
                .Include(c => c.Province)
                .Include(c => c.Country)
                .ToList();

            return Ok(cities);
        }
    }
}
