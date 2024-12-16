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
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DoctorController
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

        // Create a new doctor record
        [HttpPost("doctor")]
        public IActionResult CreateDoctor([FromBody] Doctor doctor)
        {
            if (doctor == null)
                return BadRequest("Invalid input");

            _applicationDbContext.Doctors.Add(doctor);
            _applicationDbContext.SaveChanges();

            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorId }, doctor);
        }

        // Get all doctor records
        [HttpGet("doctor")]
        public IActionResult GetAllDoctors()
        {
            var doctors = _applicationDbContext.Doctors.ToList();
            return Ok(doctors);
        }

        // Get a specific doctor record by ID
        [HttpGet("doctor/{id}")]
        public IActionResult GetDoctor(Guid id)
        {
            var doctor = _applicationDbContext.Doctors.Find(id);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        // Update a doctor record
        [HttpPut("doctor/{id}")]
        public IActionResult UpdateDoctor(Guid id, [FromBody] Doctor updatedDoctor)
        {
            var existingDoctor = _applicationDbContext.Doctors.Find(id);
            if (existingDoctor == null)
                return NotFound();

            // Update fields
            existingDoctor.KodeDokter = updatedDoctor.KodeDokter;
            existingDoctor.NamaLengkap = updatedDoctor.NamaLengkap;
            existingDoctor.NamaMarga = updatedDoctor.NamaMarga;
            existingDoctor.NomorKtpDokter = updatedDoctor.NomorKtpDokter;
            existingDoctor.TempatLahir = updatedDoctor.TempatLahir;
            existingDoctor.TanggalLahir = updatedDoctor.TanggalLahir;
            existingDoctor.JenisKelamin = updatedDoctor.JenisKelamin;
            existingDoctor.Kewarganegaraan = updatedDoctor.Kewarganegaraan;
            existingDoctor.LastEducationId = updatedDoctor.LastEducationId;
            existingDoctor.ReligionId = updatedDoctor.ReligionId;
            existingDoctor.WorkingId = updatedDoctor.WorkingId;
            existingDoctor.DoctorQueueTypeId = updatedDoctor.DoctorQueueTypeId;
            existingDoctor.BankId = updatedDoctor.BankId;
            existingDoctor.BankCabangId = updatedDoctor.BankCabangId;
            existingDoctor.BankAtasNama = updatedDoctor.BankAtasNama;
            existingDoctor.BankNomorRekening = updatedDoctor.BankNomorRekening;
            existingDoctor.Npwp = updatedDoctor.Npwp;
            existingDoctor.AlamatRumahLengkap = updatedDoctor.AlamatRumahLengkap;
            existingDoctor.CountryId = updatedDoctor.CountryId;
            existingDoctor.ProvinceId = updatedDoctor.ProvinceId;
            existingDoctor.CityId = updatedDoctor.CityId;
            existingDoctor.DistrictId = updatedDoctor.DistrictId;
            existingDoctor.SubDistrictId = updatedDoctor.SubDistrictId;
            existingDoctor.KodePos = updatedDoctor.KodePos;
            existingDoctor.NomorTelepon = updatedDoctor.NomorTelepon;
            existingDoctor.NomorHandphone = updatedDoctor.NomorHandphone;
            existingDoctor.AlamatKantorLengkap = updatedDoctor.AlamatKantorLengkap;
            existingDoctor.NomorTeleponKantor = updatedDoctor.NomorTeleponKantor;
            existingDoctor.NomorIdDokter = updatedDoctor.NomorIdDokter;
            existingDoctor.DoctorTitleId = updatedDoctor.DoctorTitleId;
            existingDoctor.JenisKontrak = updatedDoctor.JenisKontrak;
            existingDoctor.TanggalAwalKontrak = updatedDoctor.TanggalAwalKontrak;
            existingDoctor.TanggalAkhirKontrak = updatedDoctor.TanggalAkhirKontrak;
            existingDoctor.TanggalKeluar = updatedDoctor.TanggalKeluar;
            existingDoctor.GuaranteeFee = updatedDoctor.GuaranteeFee;
            existingDoctor.DokterMitra = updatedDoctor.DokterMitra;
            existingDoctor.DokterSpesialis = updatedDoctor.DokterSpesialis;
            existingDoctor.Foto = updatedDoctor.Foto;

            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        // Delete a doctor record
        [HttpDelete("doctor/{id}")]
        public IActionResult DeleteDoctor(Guid id)
        {
            var doctor = _applicationDbContext.Doctors.Find(id);
            if (doctor == null)
                return NotFound();

            _applicationDbContext.Doctors.Remove(doctor);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }
    }
}
