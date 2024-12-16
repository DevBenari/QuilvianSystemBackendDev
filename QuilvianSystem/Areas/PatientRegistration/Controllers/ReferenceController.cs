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
    public class ReferenceController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReferenceController
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
        public IActionResult GetReference()
        {

            var Reference = _applicationDbContext.References.ToList();
            if (Reference == null || !Reference.Any())
            {
                return NotFound(new { message = "Belum ada data Reference." });
            }
            return Ok(Reference);
        }
        // Create a new reference
        [HttpPost]
        public IActionResult CreateReference([FromBody] Reference reference)
        {
            if (reference == null) return BadRequest();
            _applicationDbContext.References.Add(reference);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetReference), new { id = reference.ReferenceId }, reference);
        }

        // Update a reference
        [HttpPut("{id}")]
        public IActionResult UpdateReference(Guid id, [FromBody] Reference updatedReference)
        {
            var reference = _applicationDbContext.References.Find(id);
            if (reference == null) return NotFound();

            reference.KodeRujukan = updatedReference.KodeRujukan;
            reference.NamaRujukan = updatedReference.NamaRujukan;
            reference.ReferenceType = updatedReference.ReferenceType;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete a reference
        [HttpDelete("{id}")]
        public IActionResult DeleteReference(Guid id)
        {
            var reference = _applicationDbContext.References.Find(id);
            if (reference == null) return NotFound();

            _applicationDbContext.References.Remove(reference);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }
    }
}
