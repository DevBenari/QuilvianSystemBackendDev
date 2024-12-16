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
    public class ReferenceDetailController : Controller
    {

        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReferenceDetailController
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
        public IActionResult GetReferenceDetail()
        {

            var ReferenceDetail = _applicationDbContext.ReferenceDetails.ToList();
            if (ReferenceDetail == null || !ReferenceDetail.Any())
            {
                return NotFound(new { message = "Belum ada data ReferenceDetail." });
            }
            return Ok(ReferenceDetail);
        }
        [HttpPost]
        public IActionResult CreateReferenceDetail([FromBody] ReferenceDetail referenceDetail)
        {
            if (referenceDetail == null) return BadRequest();
            _applicationDbContext.ReferenceDetails.Add(referenceDetail);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetReferenceDetail), new { id = referenceDetail.ReferenceDetailId }, referenceDetail);
        }

        // Update a reference detail
        [HttpPut("{id}")]
        public IActionResult UpdateReferenceDetail(Guid id, [FromBody] ReferenceDetail updatedReferenceDetail)
        {
            var referenceDetail = _applicationDbContext.ReferenceDetails.Find(id);
            if (referenceDetail == null) return NotFound();

            referenceDetail.KodeDetailRujukan = updatedReferenceDetail.KodeDetailRujukan;
            referenceDetail.NamaDetailRujukan = updatedReferenceDetail.NamaDetailRujukan;
            referenceDetail.NomorTelepon = updatedReferenceDetail.NomorTelepon;
            referenceDetail.Alamat = updatedReferenceDetail.Alamat;
            referenceDetail.ReferenceTypeId = updatedReferenceDetail.ReferenceTypeId;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete a reference detail
        [HttpDelete("{id}")]
        public IActionResult DeleteReferenceDetail(Guid id)
        {
            var referenceDetail = _applicationDbContext.ReferenceDetails.Find(id);
            if (referenceDetail == null) return NotFound();

            _applicationDbContext.ReferenceDetails.Remove(referenceDetail);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }
    }
}
