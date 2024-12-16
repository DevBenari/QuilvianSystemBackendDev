using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;

namespace BenariMikronWebApp.Areas.Administration.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class CountryController : Controller
    {

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CountryController
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
        public IActionResult GetCountry()
        {

            var Country = _applicationDbContext.Countries.ToList();
            if (Country == null || !Country.Any())
            {
                return NotFound(new { message = "Belum ada data Country." });
            }
            return Ok(Country);
        }

        // Get a specific country by ID
        [HttpGet("{id}")]
        public IActionResult GetCountryById(Guid id)
        {
            var country = _applicationDbContext.Countries.Find(id);
            if (country == null)
                return NotFound();

            return Ok(country);
        }

        // Create a new country
        [HttpPost]
        public IActionResult CreateCountry([FromBody] Country country)
        {
            if (country == null)
                return BadRequest("Country data is invalid.");

            _applicationDbContext.Countries.Add(country);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetCountryById), new { id = country.CountryId }, country);
        }

        // Update a country
        [HttpPut("{id}")]
        public IActionResult UpdateCountry(Guid id, [FromBody] Country updatedCountry)
        {
            var existingCountry = _applicationDbContext.Countries.Find(id);
            if (existingCountry == null)
                return NotFound();

            existingCountry.KodeNegara = updatedCountry.KodeNegara;
            existingCountry.NamaNegara = updatedCountry.NamaNegara;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a country
        [HttpDelete("{id}")]
        public IActionResult DeleteCountry(Guid id)
        {
            var country = _applicationDbContext.Countries.Find(id);
            if (country == null)
                return NotFound();

            _applicationDbContext.Countries.Remove(country);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
