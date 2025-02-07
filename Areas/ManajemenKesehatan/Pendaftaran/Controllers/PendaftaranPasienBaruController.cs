using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using System.Drawing;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace QuilvianSystemBackendDev.Areas.Pendaftaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] 
    [EnableCors("AllowSpecific")]
    public class PendaftaranPasienBaruController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<PendaftaranPasienBaruController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PendaftaranPasienBaruController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<PendaftaranPasienBaruController> logger,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _applicationDbContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: api/PendaftaranPasien
        [HttpGet]
        public IActionResult GetAllPendaftaranPasienBaru()
        {
            var daftar = _applicationDbContext.PendaftaranPasienBarus.ToList();
            if (daftar == null || !daftar.Any())
            {
                return NotFound(new { message = "Belum ada data. || 404 Not Found" });
            }

            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = daftar
            });
        }

        // GET: api/PendaftaranPasien
        [HttpGet("{id}")]
        public IActionResult GetPendaftraanPasienBaruById(Guid id)
        {
            var daftar = _applicationDbContext.PendaftaranPasienBarus.Find(id);
            if (daftar == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = daftar
            });
        }
        
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePendaftaranPasienBaru([FromForm] PendaftaranPasienBaruViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var dateNow = DateTimeOffset.Now;
                var setDateNow = dateNow.ToString("yyMMdd");

                // Ambil data terakhir untuk hari ini (tanpa ToString di query)
                var lastCode = _applicationDbContext.PendaftaranPasienBarus
                    .Where(d => d.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(k => k.KodePasien)
                    .FirstOrDefault();

                string kodePasien;
                if (lastCode == null)
                {
                    kodePasien = $"PSN{setDateNow}0001";
                }
                else
                {
                    var lastCodeTrim = lastCode.KodePasien.Substring(3, 6);

                    if (lastCodeTrim != setDateNow)
                    {
                        kodePasien = $"PSN{setDateNow}0001";
                    }
                    else
                    {
                        kodePasien = $"PSN{setDateNow}" + (Convert.ToInt32(lastCode.KodePasien.Substring(9)) + 1).ToString("D4");
                    }
                }

                // Generate Nomor Rekam Medis
                var kodeTahun = dateNow.ToString("yy");
                var kodeHari = dateNow.ToString("dd");
                var tipePasien = "10"; // Kode untuk Pasien Baru

                var lastRekamMedis = _applicationDbContext.PendaftaranPasienBarus
                    .OrderByDescending(p => p.NoRekamMedis)
                    .FirstOrDefault();

                string noRekamMedis;
                if (lastRekamMedis == null)
                {
                    noRekamMedis = $"{kodeTahun}-{kodeHari}-{tipePasien}-01";
                }
                else
                {
                    var lastNo = Convert.ToInt32(lastRekamMedis.NoRekamMedis.Substring(9)) + 1;
                    noRekamMedis = $"{kodeTahun}-{kodeHari}-{tipePasien}-{lastNo:D2}";
                }

                // Path logo untuk QR Code
                var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");

                // Generate QR Code dengan logo
                var qrCodeImage = QrCodeHelper.GenerateQRCodeWithLogo(noRekamMedis, logoPath);

                // Tentukan lokasi penyimpanan QR Code
                var qrCodeFolder = Path.Combine(_webHostEnvironment.WebRootPath, "QRCodePasienBaru");
                if (!Directory.Exists(qrCodeFolder))
                {
                    Directory.CreateDirectory(qrCodeFolder);
                }

                // Nama file QR Code berdasarkan NoRekamMedis
                var qrCodeFileName = $"{noRekamMedis}.png";
                var qrCodeFilePath = Path.Combine(qrCodeFolder, qrCodeFileName);

                // Simpan QR Code sebagai file PNG
                qrCodeImage.Save(qrCodeFilePath, System.Drawing.Imaging.ImageFormat.Png);

                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.PendaftaranPasienBarus
                    .Any(c => c.KodePasien == kodePasien && c.NamaLengkap == vm.NamaLengkap && c.NoIdentitas == vm.NoIdentitas);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Validasi & Simpan Foto Profil**
                string fotoPath = null;
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024;
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                    if (vm.Foto.Length > maxSize)
                    {
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                    }

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                    }

                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoPasienBaru");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var fotoFileName = $"{kodePasien}{fileExtension}";
                    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                    {
                        vm.Foto.CopyTo(stream);
                    }

                    fotoPath = $"/FotoPasienBaru/{fotoFileName}";
                }
                else
                {
                    //Jika user tidak upload foto, gunakan foto default
                    fotoPath = "/FotoPasienBaru/user.jpg";
                }

                // Simpan Data Pendaftaran
                var daftar = new PendaftaranPasienBaru
                {
                    PendaftaranPasienBaruId = Guid.NewGuid(),
                    CreateDateTime = DateTimeOffset.Now,
                    CreateBy = UserActiveId,
                    UpdateDateTime = DateTimeOffset.MinValue,
                    UpdateBy = Guid.Empty,
                    DeleteDateTime = DateTimeOffset.MinValue,
                    DeleteBy = Guid.Empty,
                    KodePasien = kodePasien,
                    NoRekamMedis = noRekamMedis,
                    TitleId = vm.TitleId,
                    NamaLengkap = vm.NamaLengkap,
                    IdentitasId = vm.IdentitasId,
                    NoIdentitas = vm.NoIdentitas,
                    TempatLahir = vm.TempatLahir,
                    TanggalLahir = vm.TanggalLahir,
                    JenisKelamin = vm.JenisKelamin,
                    Status = vm.Status,
                    AgamaId = vm.AgamaId,
                    PendidikanTerakhirId = vm.PendidikanTerakhirId,
                    AlamatIdentitas = vm.AlamatIdentitas,
                    AlamatDomisili = vm.AlamatDomisili,
                    NegaraId = vm.NegaraId,
                    ProvinsiId = vm.ProvinsiId,
                    KotaId = vm.KotaId,
                    KecKabId = vm.KecKabId,
                    KelurahanId = vm.KelurahanId,
                    KodePos = vm.KodePos,
                    Email = vm.Email,
                    NoTelepon1 = vm.NoTelepon1,
                    NoTelepon2 = vm.NoTelepon2,
                    NoTelepon3 = vm.NoTelepon3,
                    KewarganegaraanId = vm.KewarganegaraanId,
                    Suku = vm.Suku,
                    StatusKewarganegaraan = vm.StatusKewarganegaraan,
                    Pekerjaan = vm.Pekerjaan,
                    NamaPerusahaan = vm.NamaPerusahaan,
                    AlamatPerusahaan = vm.AlamatPerusahaan,
                    NoTeleponPerusahaan = vm.NoTeleponPerusahaan,
                    GolonganDarahId = vm.GolonganDarahId,
                    Alergi = vm.Alergi,
                    RiwayatPenyakit = vm.RiwayatPenyakit,
                    RiwayatOperasi = vm.RiwayatOperasi,
                    RiwayatPenyakitKeluarga = vm.RiwayatPenyakitKeluarga,
                    NamaKontakDarurat = vm.NamaKontakDarurat,
                    HubunganPasien = vm.HubunganPasien,
                    NoIdentitasDarurat = vm.NoIdentitasDarurat,
                    AlamatDarurat = vm.AlamatDarurat,
                    NoTeleponDarurat = vm.NoTeleponDarurat,
                    NamaOrangTua = vm.NamaOrangTua,
                    IdentitasOrangTua = vm.IdentitasOrangTua,
                    PekerjaanOrangTua = vm.PekerjaanOrangTua,
                    HubunganAnak = vm.HubunganAnak,
                    InformasiSekolah = vm.InformasiSekolah,
                    Foto = fotoPath,
                    QrCode = $"/qrcodes/{qrCodeFileName}" // Simpan hanya path QR Code
                };

                _applicationDbContext.PendaftaranPasienBarus.Add(daftar);
                _applicationDbContext.SaveChanges();

                return Created("", new
                {
                    message = "Tambah Data Berhasil || 201 Created",
                    qrCodeUrl = $"{Request.Scheme}://{Request.Host}{daftar.QrCode}",
                    uploadFotoUrl = fotoPath != null ? $"{Request.Scheme}://{Request.Host}{fotoPath}" : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // PUT: api/PendaftaranPasien/5
        [HttpPut("{id}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePendaftaranPasien(Guid id, [FromForm] PendaftaranPasienBaruViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Pasien**
                var pasien = _applicationDbContext.PendaftaranPasienBarus.Find(id);
                if (pasien == null)
                {
                    return NotFound(new { message = "Pasien tidak ditemukan." });
                }

                // **Update Data Pasien**
                pasien.TitleId = vm.TitleId ?? pasien.TitleId;
                pasien.NamaLengkap = vm.NamaLengkap ?? pasien.NamaLengkap;
                pasien.IdentitasId = vm.IdentitasId ?? pasien.IdentitasId;
                pasien.NoIdentitas = vm.NoIdentitas ?? pasien.NoIdentitas;
                pasien.TempatLahir = vm.TempatLahir ?? pasien.TempatLahir;
                pasien.TanggalLahir = vm.TanggalLahir != default ? vm.TanggalLahir : pasien.TanggalLahir;
                pasien.JenisKelamin = vm.JenisKelamin ?? pasien.JenisKelamin;
                pasien.Status = vm.Status ?? pasien.Status;
                pasien.AgamaId = vm.AgamaId ?? pasien.AgamaId;
                pasien.PendidikanTerakhirId = vm.PendidikanTerakhirId ?? pasien.PendidikanTerakhirId;
                pasien.AlamatIdentitas = vm.AlamatIdentitas ?? pasien.AlamatIdentitas;
                pasien.AlamatDomisili = vm.AlamatDomisili ?? pasien.AlamatDomisili;
                pasien.NegaraId = vm.NegaraId ?? pasien.NegaraId;
                pasien.ProvinsiId = vm.ProvinsiId ?? pasien.ProvinsiId;
                pasien.KotaId = vm.KotaId ?? pasien.KotaId;
                pasien.KecKabId = vm.KecKabId ?? pasien.KecKabId;
                pasien.KelurahanId = vm.KelurahanId ?? pasien.KelurahanId;
                pasien.KodePos = vm.KodePos ?? pasien.KodePos;
                pasien.Email = vm.Email ?? pasien.Email;
                pasien.NoTelepon1 = vm.NoTelepon1 ?? pasien.NoTelepon1;
                pasien.NoTelepon2 = vm.NoTelepon2 ?? pasien.NoTelepon2;
                pasien.NoTelepon3 = vm.NoTelepon3 ?? pasien.NoTelepon3;
                pasien.KewarganegaraanId = vm.KewarganegaraanId ?? pasien.KewarganegaraanId;
                pasien.Suku = vm.Suku ?? pasien.Suku;
                pasien.StatusKewarganegaraan = vm.StatusKewarganegaraan ?? pasien.StatusKewarganegaraan;
                pasien.Pekerjaan = vm.Pekerjaan ?? pasien.Pekerjaan;
                pasien.NamaPerusahaan = vm.NamaPerusahaan ?? pasien.NamaPerusahaan;
                pasien.AlamatPerusahaan = vm.AlamatPerusahaan ?? pasien.AlamatPerusahaan;
                pasien.NoTeleponPerusahaan = vm.NoTeleponPerusahaan ?? pasien.NoTeleponPerusahaan;
                pasien.GolonganDarahId = vm.GolonganDarahId ?? pasien.GolonganDarahId;
                pasien.Alergi = vm.Alergi ?? pasien.Alergi;
                pasien.RiwayatPenyakit = vm.RiwayatPenyakit ?? pasien.RiwayatPenyakit;
                pasien.RiwayatOperasi = vm.RiwayatOperasi ?? pasien.RiwayatOperasi;
                pasien.RiwayatPenyakitKeluarga = vm.RiwayatPenyakitKeluarga ?? pasien.RiwayatPenyakitKeluarga;
                pasien.NamaKontakDarurat = vm.NamaKontakDarurat ?? pasien.NamaKontakDarurat;
                pasien.HubunganPasien = vm.HubunganPasien ?? pasien.HubunganPasien;
                pasien.NoIdentitasDarurat = vm.NoIdentitasDarurat ?? pasien.NoIdentitasDarurat;
                pasien.AlamatDarurat = vm.AlamatDarurat ?? pasien.AlamatDarurat;
                pasien.NoTeleponDarurat = vm.NoTeleponDarurat ?? pasien.NoTeleponDarurat;
                pasien.NamaOrangTua = vm.NamaOrangTua ?? pasien.NamaOrangTua;
                pasien.IdentitasOrangTua = vm.IdentitasOrangTua ?? pasien.IdentitasOrangTua;
                pasien.PekerjaanOrangTua = vm.PekerjaanOrangTua ?? pasien.PekerjaanOrangTua;
                pasien.HubunganAnak = vm.HubunganAnak ?? pasien.HubunganAnak;
                pasien.InformasiSekolah = vm.InformasiSekolah ?? pasien.InformasiSekolah;

                // **Update Foto Profil**
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024;
                    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                    if (vm.Foto.Length > maxSize)
                    {
                        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                    }

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                    }

                    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoPasienBaru");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var fotoFileName = $"{pasien.KodePasien}{fileExtension}";
                    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                    {
                        vm.Foto.CopyTo(stream);
                    }

                    pasien.Foto = $"/FotoPasienBaru/{fotoFileName}";
                }

                // **Update QR Code jika No Rekam Medis berubah**
                //if (vm.NoRekamMedis != null && vm.NoRekamMedis != pasien.NoRekamMedis)
                //{
                //    var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");
                //    var qrCodeImage = QrCodeHelper.GenerateQRCodeWithLogo(vm.NoRekamMedis, logoPath);

                //    var qrCodeFolder = Path.Combine(_webHostEnvironment.WebRootPath, "QRCodePasienBaru");
                //    if (!Directory.Exists(qrCodeFolder))
                //    {
                //        Directory.CreateDirectory(qrCodeFolder);
                //    }

                //    var qrCodeFileName = $"{vm.NoRekamMedis}.png";
                //    var qrCodeFilePath = Path.Combine(qrCodeFolder, qrCodeFileName);
                //    qrCodeImage.Save(qrCodeFilePath, System.Drawing.Imaging.ImageFormat.Png);

                //    pasien.QrCode = $"/qrcodes/{qrCodeFileName}";
                //}

                pasien.UpdateBy = UserActiveId;
                pasien.UpdateDateTime = DateTimeOffset.Now;

                _applicationDbContext.PendaftaranPasienBarus.Update(pasien);
                _applicationDbContext.SaveChanges();

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                    qrCodeUrl = $"{Request.Scheme}://{Request.Host}{pasien.QrCode}",
                    uploadFotoUrl = $"{Request.Scheme}://{Request.Host}{pasien.Foto}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // DELETE: api/PendaftaranPasien/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePendaftaranPasien(Guid id)
        {
            try
            {
                //Ambil User ID dari JWT Claims
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // **Cari Data Pasien**
                var pasien = _applicationDbContext.PendaftaranPasienBarus.Find(id);
                if (pasien == null)
                {
                    return NotFound(new { message = "Pasien tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                pasien.DeleteBy = UserActiveId;
                pasien.DeleteDateTime = DateTimeOffset.Now;
                pasien.IsDelete = true;

                _applicationDbContext.PendaftaranPasienBarus.Update(pasien);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data pasien berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

    }
}
