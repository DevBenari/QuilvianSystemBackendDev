using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystem.Areas.PatientRegistration.Models;
using QuilvianSystem.Areas.PatientRegistration.ViewModels;
using QuilvianSystem.Models;
using QuilvianSystem.Repositories;
using System.Drawing;
using ZXing.QrCode;
using ZXing;
using Microsoft.AspNetCore.Cors;

namespace QuilvianSystem.Areas.PatientRegistration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Endpoint ini memerlukan token
    [EnableCors("AllowSpecific")]
    public class NewPatientController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewPatientController
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
        public IActionResult GetNewPatient()
        {

            var NewPatient = _applicationDbContext.NewPatients.ToList();
            if (NewPatient == null || !NewPatient.Any())
            {
                return NotFound(new { message = "Belum ada data NewPatient." });
            }
            return Ok(NewPatient);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewPatient([FromBody] CreateNewPatientViewModel newPatient)
        {
            var dateNow = DateTimeOffset.Now;
            var day = dateNow.Day;
            var month = dateNow.Month;
            var year = dateNow.Year;


            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                var writer = new QRCodeWriter();
                var resultBit = writer.encode(newPatient.NomorRekamMedisBaru, BarcodeFormat.QR_CODE, 200, 200);
                var matrix = resultBit;
                int scale = 2;
                Bitmap result = new Bitmap(matrix.Width * scale, matrix.Height * scale);
                for (int x = 0; x < matrix.Height; x++)
                {
                    for (int y = 0; y < matrix.Width; y++)
                    {
                        Color pixel = matrix[x, y] ? Color.Black : Color.White;
                        for (int i = 0; i < scale; i++)
                            for (int j = 0; j < scale; j++)
                                result.SetPixel(x * scale + i, y * scale + j, pixel);
                    }
                }

                string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "NewQRCodePasien");
                if (string.IsNullOrEmpty(_webHostEnvironment.WebRootPath))
                {
                    throw new InvalidOperationException("WebRootPath tidak diset.");
                }

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                uniqueFileName = Guid.NewGuid().ToString() + "_" + newPatient.NomorRekamMedisBaru + "_" + newPatient.NamaLengkapPasien + ".png";

                if (string.IsNullOrEmpty(uniqueFileName))
                {
                    throw new InvalidOperationException("Nama file unik bernilai null atau kosong.");
                }

                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                result.Save(filePath);

                var daftar = new NewPatient
                {
                    CreateDateTime = DateTimeOffset.Now,
                    PatientRegistrationId = newPatient.PatientRegistrationId,
                    KodePasien = newPatient.KodePasien,
                    NomorRekamMedisBaru = newPatient.NomorRekamMedisBaru,
                    NomorRekamMedisLama = newPatient.NomorRekamMedisLama,
                    TipePasien = newPatient.TipePasien,
                    InsuranceId = new Guid("8556037A-1DF7-41AC-A9DC-08DC79724C18"),
                    NomorPolis = newPatient.NomorPolis,
                    NamaLengkapPasien = newPatient.NamaLengkapPasien,
                    Title = newPatient.Title,
                    NomorIdentitasPasien = newPatient.NomorIdentitasPasien,
                    TempatLahir = newPatient.TempatLahir,
                    TanggalLahir = newPatient.TanggalLahir,
                    JenisKelamin = newPatient.JenisKelamin,
                    PasienPrioritas = newPatient.PasienPrioritas,
                    StatusPasien = newPatient.StatusPasien,
                    ReligionId = new Guid("90263116-1801-421B-B94F-08DC806C4176"),
                    Kewarganegaraan = newPatient.Kewarganegaraan,
                    LastEducationId = newPatient.LastEducationId,
                    AlamatLengkap = newPatient.AlamatLengkap,
                    CountryId = new Guid("CE20CCC9-3CC0-4D1E-A14C-08DC78893699"),
                    ProvinceId = new Guid("0612468D-8114-44B8-2882-08DC7889E86B"),
                    CityId = new Guid("34A39589-7B30-4330-3052-08DC7892E1AD"),
                    DistrictId = new Guid("2662276A-F19A-423C-EDD7-08DC7B4A0815"),
                    SubDistrictId = new Guid("F923A33F-056C-4364-CE5B-08DC789AD4D8"),
                    KodePos = newPatient.KodePos,
                    NomorTelepon1 = newPatient.NomorTelepon1,
                    NomorTelepon2 = newPatient.NomorTelepon2,
                    Email = newPatient.Email,
                    Pekerjaan = newPatient.Pekerjaan,
                    NamaKantor = newPatient.NamaKantor,
                    AlamatKantor = newPatient.AlamatKantor,
                    NomorTeleponKantor = newPatient.NomorTeleponKantor,
                    BloodTypeId = new Guid("5C3C217D-F397-45E7-0D71-08DC838498BA"),
                    Alergi = newPatient.Alergi,
                    NamaKeluargaTerdekat = newPatient.NamaKeluargaTerdekat,
                    HubunganKeluarga = newPatient.HubunganKeluarga,
                    KaryawanRumahSakit = newPatient.KaryawanRumahSakit,
                    AlamatKeluargaPasien = newPatient.AlamatKeluargaPasien,
                    NomorTeleponKeluargaPasien = newPatient.NomorTeleponKeluargaPasien,
                    NamaAyahPasien = newPatient.NamaAyahPasien,
                    PekerjaanAyahPasien = newPatient.PekerjaanAyahPasien,
                    NamaIbuPasien = newPatient.NamaIbuPasien,
                    PekerjaanIbuPasien = newPatient.PekerjaanIbuPasien,
                    NamaSutriPasien = newPatient.NamaSutriPasien,
                    PekerjaanSutriPasien = newPatient.PekerjaanSutriPasien,
                    NomorIdentitasSutriPasien = newPatient.NomorIdentitasSutriPasien,
                    GenerateQrCode = uniqueFileName
                };


                var checkDuplicate = _applicationDbContext.NewPatients
                                      .Where(c => c.KodePasien == newPatient.KodePasien)
                                      .ToList();

                if (checkDuplicate.Count == 0)
                {
                    _applicationDbContext.NewPatients.Add(daftar);
                    await _applicationDbContext.SaveChangesAsync();
                    return Ok(new { message = "Pasien berhasil ditambahkan" });
                }
                else
                {
                    return NotFound(new { message = "Terdapat duplikasi data !!!" });
                }
            }
            else
            {
                return BadRequest(new { message = "Data tidak valid !!!" });
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateNewPatient(Guid id, [FromBody] NewPatient updateNewPatient)
        {
            var patient = _applicationDbContext.NewPatients.Find(id);
            if (patient == null) return NotFound();

            // Update the patient data
            patient.KodePasien = updateNewPatient.KodePasien;
            patient.NomorRekamMedisBaru = updateNewPatient.NomorRekamMedisBaru;
            patient.NomorRekamMedisLama = updateNewPatient.NomorRekamMedisLama;
            patient.TipePasien = updateNewPatient.TipePasien;
            patient.InsuranceId = updateNewPatient.InsuranceId;
            patient.NomorPolis = updateNewPatient.NomorPolis;
            patient.NamaLengkapPasien = updateNewPatient.NamaLengkapPasien;
            patient.Title = updateNewPatient.Title;
            patient.NomorIdentitasPasien = updateNewPatient.NomorIdentitasPasien;
            patient.TempatLahir = updateNewPatient.TempatLahir;
            patient.TanggalLahir = updateNewPatient.TanggalLahir;
            patient.JenisKelamin = updateNewPatient.JenisKelamin;
            patient.PasienPrioritas = updateNewPatient.PasienPrioritas;
            patient.StatusPasien = updateNewPatient.StatusPasien;
            patient.ReligionId = updateNewPatient.ReligionId;
            patient.Kewarganegaraan = updateNewPatient.Kewarganegaraan;
            patient.LastEducationId = updateNewPatient.LastEducationId;
            patient.AlamatLengkap = updateNewPatient.AlamatLengkap;
            patient.CountryId = updateNewPatient.CountryId;
            patient.ProvinceId = updateNewPatient.ProvinceId;
            patient.CityId = updateNewPatient.CityId;
            patient.DistrictId = updateNewPatient.DistrictId;
            patient.SubDistrictId = updateNewPatient.SubDistrictId;
            patient.KodePos = updateNewPatient.KodePos;
            patient.NomorTelepon1 = updateNewPatient.NomorTelepon1;
            patient.NomorTelepon2 = updateNewPatient.NomorTelepon2;
            patient.Email = updateNewPatient.Email;
            patient.Pekerjaan = updateNewPatient.Pekerjaan;
            patient.NamaKantor = updateNewPatient.NamaKantor;
            patient.AlamatKantor = updateNewPatient.AlamatKantor;
            patient.NomorTeleponKantor = updateNewPatient.NomorTeleponKantor;
            patient.BloodTypeId = updateNewPatient.BloodTypeId;
            patient.Alergi = updateNewPatient.Alergi;
            patient.NamaKeluargaTerdekat = updateNewPatient.NamaKeluargaTerdekat;
            patient.HubunganKeluarga = updateNewPatient.HubunganKeluarga;
            patient.KaryawanRumahSakit = updateNewPatient.KaryawanRumahSakit;
            patient.AlamatKeluargaPasien = updateNewPatient.AlamatKeluargaPasien;
            patient.NomorTeleponKeluargaPasien = updateNewPatient.NomorTeleponKeluargaPasien;
            patient.NamaAyahPasien = updateNewPatient.NamaAyahPasien;
            patient.PekerjaanAyahPasien = updateNewPatient.PekerjaanAyahPasien;
            patient.NamaIbuPasien = updateNewPatient.NamaIbuPasien;
            patient.PekerjaanIbuPasien = updateNewPatient.PekerjaanIbuPasien;
            patient.NamaSutriPasien = updateNewPatient.NamaSutriPasien;
            patient.PekerjaanSutriPasien = updateNewPatient.PekerjaanSutriPasien;
            patient.NomorIdentitasSutriPasien = updateNewPatient.NomorIdentitasSutriPasien;
            patient.GenerateQrCode = updateNewPatient.GenerateQrCode;

            // Save the changes to the database
            _applicationDbContext.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteNewPatient(Guid id)
        {
            var patient = _applicationDbContext.NewPatients.Find(id);
            if (patient == null) return NotFound();

            // Remove the patient from the database
            _applicationDbContext.NewPatients.Remove(patient);
            _applicationDbContext.SaveChanges();

            return NoContent();
        }

    }
}
