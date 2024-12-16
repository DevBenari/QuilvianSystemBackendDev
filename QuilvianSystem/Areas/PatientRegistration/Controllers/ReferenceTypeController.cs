using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystem.Areas.PatientRegistration.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;

namespace QuilvianSystem.Areas.PatientRegistration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class ReferenceTypeController : Controller
    {

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReferenceTypeController
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
        public IActionResult GetReferenceType()
        {

            var ReferenceType = _applicationDbContext.ReferenceTypes.ToList();
            if (ReferenceType == null || !ReferenceType.Any())
            {
                return NotFound(new { message = "Belum ada data ReferenceType." });
            }
            return Ok(ReferenceType);
        }

        [HttpPost]
        public IActionResult CreateReferenceType([FromBody] ReferenceType referenceType)
        {
            if (referenceType == null) return BadRequest();
            _applicationDbContext.ReferenceTypes.Add(referenceType);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetReferenceType), new { id = referenceType.ReferenceTypeId }, referenceType);
        }

        // Update a reference type
        [HttpPut("{id}")]
        public IActionResult UpdateReferenceType(Guid id, [FromBody] ReferenceType updatedReferenceType)
        {
            var referenceType = _applicationDbContext.ReferenceTypes.Find(id);
            if (referenceType == null) return NotFound();

            referenceType.KodeTipeRujukan = updatedReferenceType.KodeTipeRujukan;
            referenceType.NamaTipeRujukan = updatedReferenceType.NamaTipeRujukan;
            referenceType.ReferenceId = updatedReferenceType.ReferenceId;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete a reference type
        [HttpDelete("{id}")]
        public IActionResult DeleteReferenceType(Guid id)
        {
            var referenceType = _applicationDbContext.ReferenceTypes.Find(id);
            if (referenceType == null) return NotFound();

            _applicationDbContext.ReferenceTypes.Remove(referenceType);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }
    }
}
