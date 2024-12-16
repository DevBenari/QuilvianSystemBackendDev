using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;

namespace BenariMikronWebApp.Areas.Administration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    public class InsuranceController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public InsuranceController
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

        // Create a new insurance entry
        [HttpPost("insurance")]
        public IActionResult CreateInsurance([FromBody] Insurance insurance)
        {
            if (insurance == null)
                return BadRequest("Invalid insurance data.");

            _applicationDbContext.Insurances.Add(insurance);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetInsurance), new { id = insurance.InsuranceId }, insurance);
        }

        // Get all insurance entries
        [HttpGet("insurance")]
        public IActionResult GetAllInsurances()
        {
            var insurances = _applicationDbContext.Insurances.ToList();
            return Ok(insurances);
        }

        // Get a specific insurance by ID
        [HttpGet("insurance/{id}")]
        public IActionResult GetInsurance(Guid id)
        {
            var insurance = _applicationDbContext.Insurances.Find(id);
            if (insurance == null)
                return NotFound();

            return Ok(insurance);
        }

        // Update an insurance entry
        [HttpPut("insurance/{id}")]
        public IActionResult UpdateInsurance(Guid id, [FromBody] Insurance updatedInsurance)
        {
            var existingInsurance = _applicationDbContext.Insurances.Find(id);
            if (existingInsurance == null)
                return NotFound();

            existingInsurance.KodeAsuransi = updatedInsurance.KodeAsuransi;
            existingInsurance.MulaiKerjasama = updatedInsurance.MulaiKerjasama;
            existingInsurance.AkhirKerjasama = updatedInsurance.AkhirKerjasama;
            existingInsurance.TipePerusahaan = updatedInsurance.TipePerusahaan;
            existingInsurance.NamaPerusahaan = updatedInsurance.NamaPerusahaan;
            existingInsurance.TarifGroupPerusahaan = updatedInsurance.TarifGroupPerusahaan;
            existingInsurance.Email = updatedInsurance.Email;
            existingInsurance.AkunBankKartuKredit = updatedInsurance.AkunBankKartuKredit;
            existingInsurance.KomisiKartuKredit = updatedInsurance.KomisiKartuKredit;
            existingInsurance.Diskon = updatedInsurance.Diskon;
            existingInsurance.TermasukAsuransi = updatedInsurance.TermasukAsuransi;
            existingInsurance.TermasukKaryawanRS = updatedInsurance.TermasukKaryawanRS;
            existingInsurance.Direktur = updatedInsurance.Direktur;
            existingInsurance.NamaKontak = updatedInsurance.NamaKontak;
            existingInsurance.Jabatan = updatedInsurance.Jabatan;
            existingInsurance.Bagian = updatedInsurance.Bagian;
            existingInsurance.Alamat = updatedInsurance.Alamat;
            existingInsurance.AlamatTagihan = updatedInsurance.AlamatTagihan;
            existingInsurance.CountryId = updatedInsurance.CountryId;
            existingInsurance.ProvinceId = updatedInsurance.ProvinceId;
            existingInsurance.CityId = updatedInsurance.CityId;
            existingInsurance.DistrictId = updatedInsurance.DistrictId;
            existingInsurance.SubDistrictId = updatedInsurance.SubDistrictId;
            existingInsurance.KodePos = updatedInsurance.KodePos;
            existingInsurance.NomorTelepon = updatedInsurance.NomorTelepon;
            existingInsurance.NomorFax = updatedInsurance.NomorFax;
            existingInsurance.Status = updatedInsurance.Status;
            existingInsurance.JenisKerjasama = updatedInsurance.JenisKerjasama;
            existingInsurance.JenisKontrak = updatedInsurance.JenisKontrak;
            existingInsurance.JatuhTempo = updatedInsurance.JatuhTempo;
            existingInsurance.KriteriaPembayaran = updatedInsurance.KriteriaPembayaran;
            existingInsurance.MenjaminPasienOTC = updatedInsurance.MenjaminPasienOTC;
            existingInsurance.AkunBankAtasNama = updatedInsurance.AkunBankAtasNama;
            existingInsurance.NamaBank = updatedInsurance.NamaBank;
            existingInsurance.NamaCabang = updatedInsurance.NamaCabang;
            existingInsurance.NomorRekeningBank = updatedInsurance.NomorRekeningBank;
            existingInsurance.Pinalti = updatedInsurance.Pinalti;
            existingInsurance.Keterangan = updatedInsurance.Keterangan;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete an insurance entry
        [HttpDelete("insurance/{id}")]
        public IActionResult DeleteInsurance(Guid id)
        {
            var insurance = _applicationDbContext.Insurances.Find(id);
            if (insurance == null)
                return NotFound();

            _applicationDbContext.Insurances.Remove(insurance);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
