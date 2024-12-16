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
    public class ExternalPatientController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ExternalPatientController
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

        [HttpGet("ambulance")]
        public IActionResult GetAllExternalPatientAmbulances()
        {
            var externalPatientAmbulances = _applicationDbContext.ExternalPatientAmbulances.ToList();
            return Ok(externalPatientAmbulances);
        }

        // Create a new external patient ambulance
        [HttpPost]
        [Route("ambulance")]
        public IActionResult CreateExternalPatientAmbulance([FromBody] ExternalPatientAmbulance externalPatientAmbulance)
        {
            if (externalPatientAmbulance == null)
                return BadRequest("Invalid input");

            _applicationDbContext.ExternalPatientAmbulances.Add(externalPatientAmbulance);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetExternalPatientAmbulance), new { id = externalPatientAmbulance.ExternalPatientId }, externalPatientAmbulance);
        }

        // Get an external patient ambulance by ID
        [HttpGet]
        [Route("ambulance/{id}")]
        public IActionResult GetExternalPatientAmbulance(Guid id)
        {
            var externalPatientAmbulance = _applicationDbContext.ExternalPatientAmbulances.Find(id);
            if (externalPatientAmbulance == null)
                return NotFound();

            return Ok(externalPatientAmbulance);
        }


        // Update an external patient ambulance
        [HttpPut]
        [Route("ambulance/{id}")]
        public IActionResult UpdateExternalPatientAmbulance(Guid id, [FromBody] ExternalPatientAmbulance updatedExternalPatientAmbulance)
        {
            var externalPatientAmbulance = _applicationDbContext.ExternalPatientAmbulances.Find(id);
            if (externalPatientAmbulance == null)
                return NotFound();

            // Update properties
            externalPatientAmbulance.KodePasien = updatedExternalPatientAmbulance.KodePasien;
            externalPatientAmbulance.NomorRekamMedisBaru = updatedExternalPatientAmbulance.NomorRekamMedisBaru;
            externalPatientAmbulance.NomorRekamMedisLama = updatedExternalPatientAmbulance.NomorRekamMedisLama;
            externalPatientAmbulance.Title = updatedExternalPatientAmbulance.Title;
            externalPatientAmbulance.NamaPasien = updatedExternalPatientAmbulance.NamaPasien;
            externalPatientAmbulance.NomorIdentitasPasien = updatedExternalPatientAmbulance.NomorIdentitasPasien;
            externalPatientAmbulance.TempatLahir = updatedExternalPatientAmbulance.TempatLahir;
            externalPatientAmbulance.TanggalLahir = updatedExternalPatientAmbulance.TanggalLahir;
            externalPatientAmbulance.JenisKelamin = updatedExternalPatientAmbulance.JenisKelamin;
            externalPatientAmbulance.AlamatLengkap = updatedExternalPatientAmbulance.AlamatLengkap;
            externalPatientAmbulance.CountryId = updatedExternalPatientAmbulance.CountryId;
            externalPatientAmbulance.ProvinceId = updatedExternalPatientAmbulance.ProvinceId;
            externalPatientAmbulance.CityId = updatedExternalPatientAmbulance.CityId;
            externalPatientAmbulance.DistrictId = updatedExternalPatientAmbulance.DistrictId;
            externalPatientAmbulance.SubDistrictId = updatedExternalPatientAmbulance.SubDistrictId;
            externalPatientAmbulance.KodePos = updatedExternalPatientAmbulance.KodePos;
            externalPatientAmbulance.NomorTelepon = updatedExternalPatientAmbulance.NomorTelepon;
            externalPatientAmbulance.EmailAktif = updatedExternalPatientAmbulance.EmailAktif;
            externalPatientAmbulance.Department = updatedExternalPatientAmbulance.Department;
            externalPatientAmbulance.Komponen = updatedExternalPatientAmbulance.Komponen;
            externalPatientAmbulance.DaerahTujuan = updatedExternalPatientAmbulance.DaerahTujuan;
            externalPatientAmbulance.KelebihanJarak = updatedExternalPatientAmbulance.KelebihanJarak;
            externalPatientAmbulance.KelebihanWaktu = updatedExternalPatientAmbulance.KelebihanWaktu;
            externalPatientAmbulance.Paramedis = updatedExternalPatientAmbulance.Paramedis;
            externalPatientAmbulance.AntarJemput = updatedExternalPatientAmbulance.AntarJemput;
            externalPatientAmbulance.Catatan = updatedExternalPatientAmbulance.Catatan;
            externalPatientAmbulance.GenerateQrCode = updatedExternalPatientAmbulance.GenerateQrCode;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete an external patient ambulance
        [HttpDelete]
        [Route("ambulance/{id}")]
        public IActionResult DeleteExternalPatientAmbulance(Guid id)
        {
            var externalPatientAmbulance = _applicationDbContext.ExternalPatientAmbulances.Find(id);
            if (externalPatientAmbulance == null)
                return NotFound();

            _applicationDbContext.ExternalPatientAmbulances.Remove(externalPatientAmbulance);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }



        [HttpGet("fasilitas")]
        public IActionResult GetAllExternalPatientFasilitas()
        {
            var externalPatientFasilitas = _applicationDbContext.ExternalPatientFasilitas.ToList();
            return Ok(externalPatientFasilitas);
        }
        // Create a new external patient facility record
        [HttpPost]
        [Route("fasilitas")]
        public IActionResult CreateExternalPatientFasilitas([FromBody] ExternalPatientFasilitas externalPatientFasilitas)
        {
            if (externalPatientFasilitas == null)
                return BadRequest("Invalid input");

            _applicationDbContext.ExternalPatientFasilitas.Add(externalPatientFasilitas);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetExternalPatientFasilitas), new { id = externalPatientFasilitas.ExternalPatientId }, externalPatientFasilitas);
        }

        // Get an external patient facility record by ID
        [HttpGet]
        [Route("fasilitas/{id}")]
        public IActionResult GetExternalPatientFasilitas(Guid id)
        {
            var externalPatientFasilitas = _applicationDbContext.ExternalPatientFasilitas.Find(id);
            if (externalPatientFasilitas == null)
                return NotFound();

            return Ok(externalPatientFasilitas);
        }


        // Update an external patient facility record
        [HttpPut]
        [Route("fasilitas/{id}")]
        public IActionResult UpdateExternalPatientFasilitas(Guid id, [FromBody] ExternalPatientFasilitas updatedExternalPatientFasilitas)
        {
            var externalPatientFasilitas = _applicationDbContext.ExternalPatientFasilitas.Find(id);
            if (externalPatientFasilitas == null)
                return NotFound();

            // Update properties
            externalPatientFasilitas.KodePasien = updatedExternalPatientFasilitas.KodePasien;
            externalPatientFasilitas.NomorRekamMedisBaru = updatedExternalPatientFasilitas.NomorRekamMedisBaru;
            externalPatientFasilitas.NomorRekamMedisLama = updatedExternalPatientFasilitas.NomorRekamMedisLama;
            externalPatientFasilitas.Title = updatedExternalPatientFasilitas.Title;
            externalPatientFasilitas.NamaPasien = updatedExternalPatientFasilitas.NamaPasien;
            externalPatientFasilitas.NomorIdentitasPasien = updatedExternalPatientFasilitas.NomorIdentitasPasien;
            externalPatientFasilitas.TempatLahir = updatedExternalPatientFasilitas.TempatLahir;
            externalPatientFasilitas.TanggalLahir = updatedExternalPatientFasilitas.TanggalLahir;
            externalPatientFasilitas.JenisKelamin = updatedExternalPatientFasilitas.JenisKelamin;
            externalPatientFasilitas.AlamatLengkap = updatedExternalPatientFasilitas.AlamatLengkap;
            externalPatientFasilitas.CountryId = updatedExternalPatientFasilitas.CountryId;
            externalPatientFasilitas.ProvinceId = updatedExternalPatientFasilitas.ProvinceId;
            externalPatientFasilitas.CityId = updatedExternalPatientFasilitas.CityId;
            externalPatientFasilitas.DistrictId = updatedExternalPatientFasilitas.DistrictId;
            externalPatientFasilitas.SubDistrictId = updatedExternalPatientFasilitas.SubDistrictId;
            externalPatientFasilitas.KodePos = updatedExternalPatientFasilitas.KodePos;
            externalPatientFasilitas.NomorTelepon = updatedExternalPatientFasilitas.NomorTelepon;
            externalPatientFasilitas.EmailAktif = updatedExternalPatientFasilitas.EmailAktif;
            externalPatientFasilitas.DetailTindakan = updatedExternalPatientFasilitas.DetailTindakan;
            externalPatientFasilitas.DokterPemeriksa = updatedExternalPatientFasilitas.DokterPemeriksa;
            externalPatientFasilitas.GenerateQrCode = updatedExternalPatientFasilitas.GenerateQrCode;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete an external patient facility record
        [HttpDelete]
        [Route("fasilitas/{id}")]
        public IActionResult DeleteExternalPatientFasilitas(Guid id)
        {
            var externalPatientFasilitas = _applicationDbContext.ExternalPatientFasilitas.Find(id);
            if (externalPatientFasilitas == null)
                return NotFound();

            _applicationDbContext.ExternalPatientFasilitas.Remove(externalPatientFasilitas);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }




        // Get all external patient laboratorium records
        [HttpGet("laboratorium")]
        public IActionResult GetAllExternalPatientLaboratorium()
        {
            var externalPatientLaboratoriums = _applicationDbContext.ExternalPatientLaboratoriums.ToList();
            return Ok(externalPatientLaboratoriums);
        }

        // Create a new external patient laboratorium record
        [HttpPost("laboratorium")]
        public IActionResult CreateExternalPatientLaboratorium([FromBody] ExternalPatientLaboratorium externalPatientLaboratorium)
        {
            if (externalPatientLaboratorium == null)
                return BadRequest("Invalid input");

            _applicationDbContext.ExternalPatientLaboratoriums.Add(externalPatientLaboratorium);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetExternalPatientLaboratorium), new { id = externalPatientLaboratorium.ExternalPatientId }, externalPatientLaboratorium);
        }

        // Get an external patient laboratorium record by ID
        [HttpGet("laboratorium/{id}")]
        public IActionResult GetExternalPatientLaboratorium(Guid id)
        {
            var externalPatientLaboratorium = _applicationDbContext.ExternalPatientLaboratoriums.Find(id);
            if (externalPatientLaboratorium == null)
                return NotFound();

            return Ok(externalPatientLaboratorium);
        }

        // Update an external patient laboratorium record
        [HttpPut("laboratorium/{id}")]
        public IActionResult UpdateExternalPatientLaboratorium(Guid id, [FromBody] ExternalPatientLaboratorium updatedExternalPatientLaboratorium)
        {
            var externalPatientLaboratorium = _applicationDbContext.ExternalPatientLaboratoriums.Find(id);
            if (externalPatientLaboratorium == null)
                return NotFound();

            // Update properties
            externalPatientLaboratorium.KodePasien = updatedExternalPatientLaboratorium.KodePasien;
            externalPatientLaboratorium.NomorRekamMedisBaru = updatedExternalPatientLaboratorium.NomorRekamMedisBaru;
            externalPatientLaboratorium.NomorRekamMedisLama = updatedExternalPatientLaboratorium.NomorRekamMedisLama;
            externalPatientLaboratorium.TipePasien = updatedExternalPatientLaboratorium.TipePasien;
            externalPatientLaboratorium.InsuranceId = updatedExternalPatientLaboratorium.InsuranceId;
            externalPatientLaboratorium.NomorPolis = updatedExternalPatientLaboratorium.NomorPolis;
            externalPatientLaboratorium.Title = updatedExternalPatientLaboratorium.Title;
            externalPatientLaboratorium.NamaPasien = updatedExternalPatientLaboratorium.NamaPasien;
            externalPatientLaboratorium.NomorIdentitasPasien = updatedExternalPatientLaboratorium.NomorIdentitasPasien;
            externalPatientLaboratorium.TempatLahir = updatedExternalPatientLaboratorium.TempatLahir;
            externalPatientLaboratorium.TanggalLahir = updatedExternalPatientLaboratorium.TanggalLahir;
            externalPatientLaboratorium.JenisKelamin = updatedExternalPatientLaboratorium.JenisKelamin;
            externalPatientLaboratorium.AlamatLengkap = updatedExternalPatientLaboratorium.AlamatLengkap;
            externalPatientLaboratorium.CountryId = updatedExternalPatientLaboratorium.CountryId;
            externalPatientLaboratorium.ProvinceId = updatedExternalPatientLaboratorium.ProvinceId;
            externalPatientLaboratorium.CityId = updatedExternalPatientLaboratorium.CityId;
            externalPatientLaboratorium.DistrictId = updatedExternalPatientLaboratorium.DistrictId;
            externalPatientLaboratorium.SubDistrictId = updatedExternalPatientLaboratorium.SubDistrictId;
            externalPatientLaboratorium.KodePos = updatedExternalPatientLaboratorium.KodePos;
            externalPatientLaboratorium.NomorTelepon = updatedExternalPatientLaboratorium.NomorTelepon;
            externalPatientLaboratorium.EmailAktif = updatedExternalPatientLaboratorium.EmailAktif;
            externalPatientLaboratorium.TipeRujukan = updatedExternalPatientLaboratorium.TipeRujukan;
            externalPatientLaboratorium.DeskripsiRujukan = updatedExternalPatientLaboratorium.DeskripsiRujukan;
            externalPatientLaboratorium.PromoId = updatedExternalPatientLaboratorium.PromoId;
            externalPatientLaboratorium.TipePemeriksaan = updatedExternalPatientLaboratorium.TipePemeriksaan;
            externalPatientLaboratorium.SuratRujukan = updatedExternalPatientLaboratorium.SuratRujukan;
            externalPatientLaboratorium.DiagnosaAwal = updatedExternalPatientLaboratorium.DiagnosaAwal;
            externalPatientLaboratorium.TanggalSampling = updatedExternalPatientLaboratorium.TanggalSampling;
            externalPatientLaboratorium.DetailTindakan = updatedExternalPatientLaboratorium.DetailTindakan;
            externalPatientLaboratorium.DokterPemeriksa = updatedExternalPatientLaboratorium.DokterPemeriksa;
            externalPatientLaboratorium.Pemeriksaan = updatedExternalPatientLaboratorium.Pemeriksaan;
            externalPatientLaboratorium.GenerateQrCode = updatedExternalPatientLaboratorium.GenerateQrCode;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete an external patient laboratorium record
        [HttpDelete("laboratorium/{id}")]
        public IActionResult DeleteExternalPatientLaboratorium(Guid id)
        {
            var externalPatientLaboratorium = _applicationDbContext.ExternalPatientLaboratoriums.Find(id);
            if (externalPatientLaboratorium == null)
                return NotFound();

            _applicationDbContext.ExternalPatientLaboratoriums.Remove(externalPatientLaboratorium);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }




        // Get all external patient medical check-up records
        [HttpGet("medical-checkup")]
        public IActionResult GetAllExternalPatientMedicalCheckUp()
        {
            var externalPatientMedicalCheckUps = _applicationDbContext.ExternalPatientMedicalCheckUps.ToList();
            return Ok(externalPatientMedicalCheckUps);
        }

        // Create a new external patient medical check-up record
        [HttpPost("medical-checkup")]
        public IActionResult CreateExternalPatientMedicalCheckUp([FromBody] ExternalPatientMedicalCheckUp externalPatientMedicalCheckUp)
        {
            if (externalPatientMedicalCheckUp == null)
                return BadRequest("Invalid input");

            _applicationDbContext.ExternalPatientMedicalCheckUps.Add(externalPatientMedicalCheckUp);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetExternalPatientMedicalCheckUp), new { id = externalPatientMedicalCheckUp.ExternalPatientId }, externalPatientMedicalCheckUp);
        }

        // Get an external patient medical check-up record by ID
        [HttpGet("medical-checkup/{id}")]
        public IActionResult GetExternalPatientMedicalCheckUp(Guid id)
        {
            var externalPatientMedicalCheckUp = _applicationDbContext.ExternalPatientMedicalCheckUps.Find(id);
            if (externalPatientMedicalCheckUp == null)
                return NotFound();

            return Ok(externalPatientMedicalCheckUp);
        }

        // Update an external patient medical check-up record
        [HttpPut("medical-checkup/{id}")]
        public IActionResult UpdateExternalPatientMedicalCheckUp(Guid id, [FromBody] ExternalPatientMedicalCheckUp updatedExternalPatientMedicalCheckUp)
        {
            var externalPatientMedicalCheckUp = _applicationDbContext.ExternalPatientMedicalCheckUps.Find(id);
            if (externalPatientMedicalCheckUp == null)
                return NotFound();

            // Update properties based on the provided updated model
            externalPatientMedicalCheckUp.KodePasien = updatedExternalPatientMedicalCheckUp.KodePasien;
            externalPatientMedicalCheckUp.NomorRekamMedisBaru = updatedExternalPatientMedicalCheckUp.NomorRekamMedisBaru;
            externalPatientMedicalCheckUp.NomorRekamMedisLama = updatedExternalPatientMedicalCheckUp.NomorRekamMedisLama;
            externalPatientMedicalCheckUp.TipePasien = updatedExternalPatientMedicalCheckUp.TipePasien;
            externalPatientMedicalCheckUp.InsuranceId = updatedExternalPatientMedicalCheckUp.InsuranceId;
            externalPatientMedicalCheckUp.NomorPolis = updatedExternalPatientMedicalCheckUp.NomorPolis;
            externalPatientMedicalCheckUp.Title = updatedExternalPatientMedicalCheckUp.Title;
            externalPatientMedicalCheckUp.NamaPasien = updatedExternalPatientMedicalCheckUp.NamaPasien;
            externalPatientMedicalCheckUp.NomorIdentitasPasien = updatedExternalPatientMedicalCheckUp.NomorIdentitasPasien;
            externalPatientMedicalCheckUp.TempatLahir = updatedExternalPatientMedicalCheckUp.TempatLahir;
            externalPatientMedicalCheckUp.TanggalLahir = updatedExternalPatientMedicalCheckUp.TanggalLahir;
            externalPatientMedicalCheckUp.JenisKelamin = updatedExternalPatientMedicalCheckUp.JenisKelamin;
            externalPatientMedicalCheckUp.AlamatLengkap = updatedExternalPatientMedicalCheckUp.AlamatLengkap;
            externalPatientMedicalCheckUp.CountryId = updatedExternalPatientMedicalCheckUp.CountryId;
            externalPatientMedicalCheckUp.ProvinceId = updatedExternalPatientMedicalCheckUp.ProvinceId;
            externalPatientMedicalCheckUp.CityId = updatedExternalPatientMedicalCheckUp.CityId;
            externalPatientMedicalCheckUp.DistrictId = updatedExternalPatientMedicalCheckUp.DistrictId;
            externalPatientMedicalCheckUp.SubDistrictId = updatedExternalPatientMedicalCheckUp.SubDistrictId;
            externalPatientMedicalCheckUp.KodePos = updatedExternalPatientMedicalCheckUp.KodePos;
            externalPatientMedicalCheckUp.NomorTelepon = updatedExternalPatientMedicalCheckUp.NomorTelepon;
            externalPatientMedicalCheckUp.EmailAktif = updatedExternalPatientMedicalCheckUp.EmailAktif;
            externalPatientMedicalCheckUp.PaketMCU = updatedExternalPatientMedicalCheckUp.PaketMCU;
            externalPatientMedicalCheckUp.DokterMCU = updatedExternalPatientMedicalCheckUp.DokterMCU;
            externalPatientMedicalCheckUp.TipeRujukan = updatedExternalPatientMedicalCheckUp.TipeRujukan;
            externalPatientMedicalCheckUp.DeskripsiRujukan = updatedExternalPatientMedicalCheckUp.DeskripsiRujukan;
            externalPatientMedicalCheckUp.Promo = updatedExternalPatientMedicalCheckUp.Promo;
            externalPatientMedicalCheckUp.SuratRujukan = updatedExternalPatientMedicalCheckUp.SuratRujukan;
            externalPatientMedicalCheckUp.DiagnosaAwal = updatedExternalPatientMedicalCheckUp.DiagnosaAwal;
            externalPatientMedicalCheckUp.GenerateQrCode = updatedExternalPatientMedicalCheckUp.GenerateQrCode;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete an external patient medical check-up record
        [HttpDelete("medical-checkup/{id}")]
        public IActionResult DeleteExternalPatientMedicalCheckUp(Guid id)
        {
            var externalPatientMedicalCheckUp = _applicationDbContext.ExternalPatientMedicalCheckUps.Find(id);
            if (externalPatientMedicalCheckUp == null)
                return NotFound();

            _applicationDbContext.ExternalPatientMedicalCheckUps.Remove(externalPatientMedicalCheckUp);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }



        // Get all external patient optik records
        [HttpGet("optik")]
        public IActionResult GetAllExternalPatientOptik()
        {
            var externalPatientOptiks = _applicationDbContext.ExternalPatientOptiks.ToList();
            return Ok(externalPatientOptiks);
        }

        // Create a new external patient optik record
        [HttpPost("optik")]
        public IActionResult CreateExternalPatientOptik([FromBody] ExternalPatientOptik externalPatientOptik)
        {
            if (externalPatientOptik == null)
                return BadRequest("Invalid input");

            _applicationDbContext.ExternalPatientOptiks.Add(externalPatientOptik);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetExternalPatientOptik), new { id = externalPatientOptik.ExternalPatientId }, externalPatientOptik);
        }


        // Get an external patient optik record by ID
        [HttpGet("optik/{id}")]
        public IActionResult GetExternalPatientOptik(Guid id)
        {
            var externalPatientOptik = _applicationDbContext.ExternalPatientOptiks.Find(id);
            if (externalPatientOptik == null)
                return NotFound();

            return Ok(externalPatientOptik);
        }

        // Update an external patient optik record
        [HttpPut("optik/{id}")]
        public IActionResult UpdateExternalPatientOptik(Guid id, [FromBody] ExternalPatientOptik updatedExternalPatientOptik)
        {
            var externalPatientOptik = _applicationDbContext.ExternalPatientOptiks.Find(id);
            if (externalPatientOptik == null)
                return NotFound();

            // Update the fields of the existing record
            externalPatientOptik.KodePasien = updatedExternalPatientOptik.KodePasien;
            externalPatientOptik.NomorRekamMedisBaru = updatedExternalPatientOptik.NomorRekamMedisBaru;
            externalPatientOptik.NomorRekamMedisLama = updatedExternalPatientOptik.NomorRekamMedisLama;
            externalPatientOptik.Title = updatedExternalPatientOptik.Title;
            externalPatientOptik.NamaPasien = updatedExternalPatientOptik.NamaPasien;
            externalPatientOptik.NomorIdentitasPasien = updatedExternalPatientOptik.NomorIdentitasPasien;
            externalPatientOptik.TempatLahir = updatedExternalPatientOptik.TempatLahir;
            externalPatientOptik.TanggalLahir = updatedExternalPatientOptik.TanggalLahir;
            externalPatientOptik.JenisKelamin = updatedExternalPatientOptik.JenisKelamin;
            externalPatientOptik.AlamatLengkap = updatedExternalPatientOptik.AlamatLengkap;
            externalPatientOptik.CountryId = updatedExternalPatientOptik.CountryId;
            externalPatientOptik.ProvinceId = updatedExternalPatientOptik.ProvinceId;
            externalPatientOptik.CityId = updatedExternalPatientOptik.CityId;
            externalPatientOptik.DistrictId = updatedExternalPatientOptik.DistrictId;
            externalPatientOptik.SubDistrictId = updatedExternalPatientOptik.SubDistrictId;
            externalPatientOptik.KodePos = updatedExternalPatientOptik.KodePos;
            externalPatientOptik.NomorTelepon = updatedExternalPatientOptik.NomorTelepon;
            externalPatientOptik.EmailAktif = updatedExternalPatientOptik.EmailAktif;
            externalPatientOptik.DetailTindakan = updatedExternalPatientOptik.DetailTindakan;
            externalPatientOptik.DokterPemeriksa = updatedExternalPatientOptik.DokterPemeriksa;
            externalPatientOptik.GenerateQrCode = updatedExternalPatientOptik.GenerateQrCode;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete an external patient optik record
        [HttpDelete("optik/{id}")]
        public IActionResult DeleteExternalPatientOptik(Guid id)
        {
            var externalPatientOptik = _applicationDbContext.ExternalPatientOptiks.Find(id);
            if (externalPatientOptik == null)
                return NotFound();

            _applicationDbContext.ExternalPatientOptiks.Remove(externalPatientOptik);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }


        // Get all external patient radiology records
        [HttpGet("radiologi")]
        public IActionResult GetAllExternalPatientRadiologi()
        {
            var externalPatientRadiologis = _applicationDbContext.ExternalPatientRadiologis.ToList();
            return Ok(externalPatientRadiologis);
        }
        // Create a new external patient radiology record
        [HttpPost("radiologi")]
        public IActionResult CreateExternalPatientRadiologi([FromBody] ExternalPatientRadiologi externalPatientRadiologi)
        {
            if (externalPatientRadiologi == null)
                return BadRequest("Invalid input");

            _applicationDbContext.ExternalPatientRadiologis.Add(externalPatientRadiologi);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetExternalPatientRadiologi), new { id = externalPatientRadiologi.ExternalPatientId }, externalPatientRadiologi);
        }

        // Get an external patient radiology record by ID
        [HttpGet("radiologi/{id}")]
        public IActionResult GetExternalPatientRadiologi(Guid id)
        {
            var externalPatientRadiologi = _applicationDbContext.ExternalPatientRadiologis.Find(id);
            if (externalPatientRadiologi == null)
                return NotFound();

            return Ok(externalPatientRadiologi);
        }

        // Update an external patient radiology record
        [HttpPut("radiologi/{id}")]
        public IActionResult UpdateExternalPatientRadiologi(Guid id, [FromBody] ExternalPatientRadiologi updatedExternalPatientRadiologi)
        {
            var externalPatientRadiologi = _applicationDbContext.ExternalPatientRadiologis.Find(id);
            if (externalPatientRadiologi == null)
                return NotFound();

            // Update the fields of the existing record
            externalPatientRadiologi.KodePasien = updatedExternalPatientRadiologi.KodePasien;
            externalPatientRadiologi.NomorRekamMedisBaru = updatedExternalPatientRadiologi.NomorRekamMedisBaru;
            externalPatientRadiologi.NomorRekamMedisLama = updatedExternalPatientRadiologi.NomorRekamMedisLama;
            externalPatientRadiologi.TipePasien = updatedExternalPatientRadiologi.TipePasien;
            externalPatientRadiologi.InsuranceId = updatedExternalPatientRadiologi.InsuranceId;
            externalPatientRadiologi.NomorPolis = updatedExternalPatientRadiologi.NomorPolis;
            externalPatientRadiologi.Title = updatedExternalPatientRadiologi.Title;
            externalPatientRadiologi.NamaPasien = updatedExternalPatientRadiologi.NamaPasien;
            externalPatientRadiologi.NomorIdentitasPasien = updatedExternalPatientRadiologi.NomorIdentitasPasien;
            externalPatientRadiologi.TempatLahir = updatedExternalPatientRadiologi.TempatLahir;
            externalPatientRadiologi.TanggalLahir = updatedExternalPatientRadiologi.TanggalLahir;
            externalPatientRadiologi.JenisKelamin = updatedExternalPatientRadiologi.JenisKelamin;
            externalPatientRadiologi.AlamatLengkap = updatedExternalPatientRadiologi.AlamatLengkap;
            externalPatientRadiologi.CountryId = updatedExternalPatientRadiologi.CountryId;
            externalPatientRadiologi.ProvinceId = updatedExternalPatientRadiologi.ProvinceId;
            externalPatientRadiologi.CityId = updatedExternalPatientRadiologi.CityId;
            externalPatientRadiologi.DistrictId = updatedExternalPatientRadiologi.DistrictId;
            externalPatientRadiologi.SubDistrictId = updatedExternalPatientRadiologi.SubDistrictId;
            externalPatientRadiologi.KodePos = updatedExternalPatientRadiologi.KodePos;
            externalPatientRadiologi.NomorTelepon = updatedExternalPatientRadiologi.NomorTelepon;
            externalPatientRadiologi.EmailAktif = updatedExternalPatientRadiologi.EmailAktif;
            externalPatientRadiologi.TipeRujukan = updatedExternalPatientRadiologi.TipeRujukan;
            externalPatientRadiologi.DeskripsiRujukan = updatedExternalPatientRadiologi.DeskripsiRujukan;
            externalPatientRadiologi.Promo = updatedExternalPatientRadiologi.Promo;
            externalPatientRadiologi.SuratRujukan = updatedExternalPatientRadiologi.SuratRujukan;
            externalPatientRadiologi.DiagnosaAwal = updatedExternalPatientRadiologi.DiagnosaAwal;
            externalPatientRadiologi.DetailTindakan = updatedExternalPatientRadiologi.DetailTindakan;
            externalPatientRadiologi.DokterPemeriksa = updatedExternalPatientRadiologi.DokterPemeriksa;
            externalPatientRadiologi.Pemeriksaan = updatedExternalPatientRadiologi.Pemeriksaan;
            externalPatientRadiologi.GenerateQrCode = updatedExternalPatientRadiologi.GenerateQrCode;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete an external patient radiology record
        [HttpDelete("radiologi/{id}")]
        public IActionResult DeleteExternalPatientRadiologi(Guid id)
        {
            var externalPatientRadiologi = _applicationDbContext.ExternalPatientRadiologis.Find(id);
            if (externalPatientRadiologi == null)
                return NotFound();

            _applicationDbContext.ExternalPatientRadiologis.Remove(externalPatientRadiologi);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }



        // Get all external patient rehabilitasi medik records
        [HttpGet("rehabilitasi")]
        public IActionResult GetAllExternalPatientRehabilitasiMedik()
        {
            var externalPatientRehabilitasiMediks = _applicationDbContext.ExternalPatientRehabilitasiMediks.ToList();
            return Ok(externalPatientRehabilitasiMediks);
        }
        // Create a new external patient rehabilitasi medik record
        [HttpPost("rehabilitasi")]
        public IActionResult CreateExternalPatientRehabilitasiMedik([FromBody] ExternalPatientRehabilitasiMedik externalPatientRehabilitasiMedik)
        {
            if (externalPatientRehabilitasiMedik == null)
                return BadRequest("Invalid input");

            _applicationDbContext.ExternalPatientRehabilitasiMediks.Add(externalPatientRehabilitasiMedik);
            _applicationDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetExternalPatientRehabilitasiMedik), new { id = externalPatientRehabilitasiMedik.ExternalPatientId }, externalPatientRehabilitasiMedik);
        }

        // Get an external patient rehabilitasi medik record by ID
        [HttpGet("rehabilitasi/{id}")]
        public IActionResult GetExternalPatientRehabilitasiMedik(Guid id)
        {
            var externalPatientRehabilitasiMedik = _applicationDbContext.ExternalPatientRehabilitasiMediks.Find(id);
            if (externalPatientRehabilitasiMedik == null)
                return NotFound();

            return Ok(externalPatientRehabilitasiMedik);
        }

        // Update an external patient rehabilitasi medik record
        [HttpPut("rehabilitasi/{id}")]
        public IActionResult UpdateExternalPatientRehabilitasiMedik(Guid id, [FromBody] ExternalPatientRehabilitasiMedik updatedExternalPatientRehabilitasiMedik)
        {
            var externalPatientRehabilitasiMedik = _applicationDbContext.ExternalPatientRehabilitasiMediks.Find(id);
            if (externalPatientRehabilitasiMedik == null)
                return NotFound();

            // Update the fields of the existing record
            externalPatientRehabilitasiMedik.KodePasien = updatedExternalPatientRehabilitasiMedik.KodePasien;
            externalPatientRehabilitasiMedik.NomorRekamMedisBaru = updatedExternalPatientRehabilitasiMedik.NomorRekamMedisBaru;
            externalPatientRehabilitasiMedik.NomorRekamMedisLama = updatedExternalPatientRehabilitasiMedik.NomorRekamMedisLama;
            externalPatientRehabilitasiMedik.TipePasien = updatedExternalPatientRehabilitasiMedik.TipePasien;
            externalPatientRehabilitasiMedik.InsuranceId = updatedExternalPatientRehabilitasiMedik.InsuranceId;
            externalPatientRehabilitasiMedik.NomorPolis = updatedExternalPatientRehabilitasiMedik.NomorPolis;
            externalPatientRehabilitasiMedik.Title = updatedExternalPatientRehabilitasiMedik.Title;
            externalPatientRehabilitasiMedik.NamaPasien = updatedExternalPatientRehabilitasiMedik.NamaPasien;
            externalPatientRehabilitasiMedik.NomorIdentitasPasien = updatedExternalPatientRehabilitasiMedik.NomorIdentitasPasien;
            externalPatientRehabilitasiMedik.TempatLahir = updatedExternalPatientRehabilitasiMedik.TempatLahir;
            externalPatientRehabilitasiMedik.TanggalLahir = updatedExternalPatientRehabilitasiMedik.TanggalLahir;
            externalPatientRehabilitasiMedik.JenisKelamin = updatedExternalPatientRehabilitasiMedik.JenisKelamin;
            externalPatientRehabilitasiMedik.AlamatLengkap = updatedExternalPatientRehabilitasiMedik.AlamatLengkap;
            externalPatientRehabilitasiMedik.CountryId = updatedExternalPatientRehabilitasiMedik.CountryId;
            externalPatientRehabilitasiMedik.ProvinceId = updatedExternalPatientRehabilitasiMedik.ProvinceId;
            externalPatientRehabilitasiMedik.CityId = updatedExternalPatientRehabilitasiMedik.CityId;
            externalPatientRehabilitasiMedik.DistrictId = updatedExternalPatientRehabilitasiMedik.DistrictId;
            externalPatientRehabilitasiMedik.SubDistrictId = updatedExternalPatientRehabilitasiMedik.SubDistrictId;
            externalPatientRehabilitasiMedik.KodePos = updatedExternalPatientRehabilitasiMedik.KodePos;
            externalPatientRehabilitasiMedik.NomorTelepon = updatedExternalPatientRehabilitasiMedik.NomorTelepon;
            externalPatientRehabilitasiMedik.EmailAktif = updatedExternalPatientRehabilitasiMedik.EmailAktif;
            externalPatientRehabilitasiMedik.TipeRujukan = updatedExternalPatientRehabilitasiMedik.TipeRujukan;
            externalPatientRehabilitasiMedik.DeskripsiRujukan = updatedExternalPatientRehabilitasiMedik.DeskripsiRujukan;
            externalPatientRehabilitasiMedik.SuratRujukan = updatedExternalPatientRehabilitasiMedik.SuratRujukan;
            externalPatientRehabilitasiMedik.DiagnosaAwal = updatedExternalPatientRehabilitasiMedik.DiagnosaAwal;
            externalPatientRehabilitasiMedik.DaftarTindakan = updatedExternalPatientRehabilitasiMedik.DaftarTindakan;
            externalPatientRehabilitasiMedik.DokterPemeriksa = updatedExternalPatientRehabilitasiMedik.DokterPemeriksa;
            externalPatientRehabilitasiMedik.GenerateQrCode = updatedExternalPatientRehabilitasiMedik.GenerateQrCode;

            _applicationDbContext.SaveChanges();
            return NoContent();
        }

        // Delete an external patient rehabilitasi medik record
        [HttpDelete("rehabilitasi/{id}")]
        public IActionResult DeleteExternalPatientRehabilitasiMedik(Guid id)
        {
            var externalPatientRehabilitasiMedik = _applicationDbContext.ExternalPatientRehabilitasiMediks.Find(id);
            if (externalPatientRehabilitasiMedik == null)
                return NotFound();

            _applicationDbContext.ExternalPatientRehabilitasiMediks.Remove(externalPatientRehabilitasiMedik);
            _applicationDbContext.SaveChanges();
            return NoContent();
        }

    }
}
