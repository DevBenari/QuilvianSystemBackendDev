using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.ViewModels;
using System.Security.Claims;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using Humanizer;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackendDev.Areas.Pendaftaran.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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

        [HttpGet]
        public async Task<IActionResult> GetAllPendaftaranPasienBaru(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = from a in _applicationDbContext.PendaftaranPasienBarus
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienBaruId = a.PendaftaranPasienBaruId,
                            KodePasien = a.KodePasien,
                            NoRekamMedis = a.NoRekamMedis,
                            NamaLengkap = a.NamaLengkap,
                            JenisKelamin = a.JenisKelamin,
                            FotoName = a.FotoName,
                            ImageBytes = a.ImageBytes,
                            FotoPath = a.FotoPath,
                        };

            // Hitung total data sebelum paginasi
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!listdata.Any())
            {
                return NotFound(new { message = "Belum ada data atau halaman tidak ditemukan. || 404 Not Found" });
            }

            // Return hasil dengan paging info
            return Ok(new
            {
                message = "Berhasil || 200 OK",
                data = listdata,
                pagination = new
                {
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalRows = totalRows,
                    TotalPages = totalPages
                }
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetPendaftraanPasienBaruById(Guid id)
        {
            var listdata = _applicationDbContext.PendaftaranPasienBarus.Find(id);
            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = listdata
            });
        }

        //[HttpGet("get-image/{id}")]
        //public async Task<IActionResult> GetImage(Guid id)
        //{
        //    var data = await _applicationDbContext.PendaftaranPasienBarus.FindAsync(id);

        //    if (data == null || data.ImageBytes == null || data.ImageBytes.Length == 0)
        //    {
        //        return NotFound(new { message = "Data tidak ditemukan atau tidak memiliki gambar." });
        //    }

        //    //string detectedFormat = GetImageFormat(data.ImageBytes);
        //    //string mimeType = detectedFormat == "image/png" ? "image/png" : "image/jpeg";

        //    return File(data.ImageBytes, mimeType); // Mengembalikan gambar dengan format yang sesuai
        //}

        //public static string GetImageExtension(string base64String)
        //{
        //    if (base64String.StartsWith("data:image/jpeg") || base64String.StartsWith("data:image/jpg"))
        //        return "jpg";
        //    if (base64String.StartsWith("data:image/png"))
        //        return "png";
        //    if (base64String.StartsWith("data:image/gif"))
        //        return "gif";
        //    if (base64String.StartsWith("data:image/bmp"))
        //        return "bmp";
        //    if (base64String.StartsWith("data:image/webp"))
        //        return "webp";

        //    return string.Empty; // Jika format tidak dikenali
        //}

        //public static string GetImageFormat(byte[] fileBytes)
        //{
        //    if (fileBytes.Length < 4) return "Unknown";

        //    if (fileBytes[0] == 0xFF && fileBytes[1] == 0xD8 && fileBytes[2] == 0xFF) return "image/jpeg";
        //    if (fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && fileBytes[2] == 0x4E && fileBytes[3] == 0x47) return "image/png";

        //    return "Unknown";
        //}

        [HttpPost]
        public async Task<IActionResult> CreatePendaftaranPasienBaru([FromBody] PendaftaranPasienBaruViewModel vm)
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
                //string fotoPath = null;
                //if (vm.Foto != null && vm.Foto.Length > 0)
                //{
                //    var maxSize = 2 * 1024 * 1024;
                //    var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
                //    var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

                //    if (vm.Foto.Length > maxSize)
                //    {
                //        return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
                //    }

                //    if (!allowedExtensions.Contains(fileExtension))
                //    {
                //        return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
                //    }

                //    var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoPasienBaru");
                //    if (!Directory.Exists(uploadFolder))
                //    {
                //        Directory.CreateDirectory(uploadFolder);
                //    }

                //    var fotoFileName = $"{kodePasien}{fileExtension}";
                //    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                //    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                //    {
                //        vm.Foto.CopyTo(stream);
                //    }

                //    fotoPath = $"/FotoPasienBaru/{fotoFileName}";
                //}
                //else
                //{
                //    //Jika user tidak upload foto, gunakan foto default
                //    fotoPath = "/FotoPasienBaru/user.jpg";
                //}

                // kode upload gambar dgn Base64
                // Dapatkan ekstensi file berdasarkan Base64
                //string extension = GetImageExtension(request.Base64Data);
                //if (string.IsNullOrEmpty(extension))
                //{
                //    return BadRequest(new { message = "Invalid image format. Allowed formats: jpg, jpeg, png, gif, bmp, webp." });
                //}

                // Folder penyimpanan (wwwroot/uploads)
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FotoPasienBaru");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }


                //// Nama file unik
                string uniqueFileName = $"{kodePasien}_{vm.NamaLengkap}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                //// Hapus prefix base64 sebelum decoding
                //var base64Data = Regex.Replace(request.Base64Data, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);

                //// Konversi base64 menjadi byte array
                //var imageBytes = Convert.FromBase64String(base64Data);

                // Simpan file ke server
                await System.IO.File.WriteAllBytesAsync(filePath, vm.ImageBytes);

                //// URL file yang disimpan
                //var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";



                // Validasi format gambar dari nama file
                //byte[] imageBytes = vm.FotoByte.ToArray();
                //string detectedFormat = GetImageFormat(imageBytes);

                //if (detectedFormat == "Unknown")
                //{
                //    return BadRequest(new { message = "Format gambar tidak didukung. Hanya menerima JPG dan PNG." });
                //}

                
                if (ModelState.IsValid)
                {
                    // Simpan Data
                    var daftar = new PendaftaranPasienBaru
                    {
                        PendaftaranPasienBaruId = Guid.NewGuid(),
                        CreateDateTime = DateTimeOffset.Now,
                        CreateBy = UserActiveId,
                        KodePasien = kodePasien,
                        NoRekamMedis = noRekamMedis,
                        TipePasien = vm.TipePasien,
                        NoRekamMedisLama = vm.NoRekamMedisLama,
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
                        PekerjaanId = vm.PekerjaanId,
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
                        FotoName = uniqueFileName,
                        QrCode = $"/qrcodes/{qrCodeFileName}", // Simpan hanya path QR Code
                        FotoPath = filePath,
                        ImageBytes = vm.ImageBytes
                    };
                    _applicationDbContext.PendaftaranPasienBarus.Add(daftar);
                    _applicationDbContext.SaveChanges();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                        qrCodeUrl = $"{Request.Scheme}://{Request.Host}{daftar.QrCode}",
                        uploadFotoUrl = $"{Request.Scheme}://{Request.Host}{daftar.FotoPath}"
                    });
                }
                else
                {
                    return BadRequest(new { message = "Data tidak valid." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePendaftaranPasien(Guid id, [FromBody] PendaftaranPasienBaruViewModel vm)
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
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                //byte[] imageBytes = vm.FotoByte.ToArray();
                //string detectedFormat = GetImageFormat(imageBytes);

                //if (detectedFormat == "Unknown")
                //{
                //    return BadRequest(new { message = "Format gambar tidak didukung. Hanya menerima JPG dan PNG." });
                //}

                // Hapus gambar lama
                var oldFilePath = vm.FotoPath;
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                // Simpan gambar baru
                await System.IO.File.WriteAllBytesAsync(oldFilePath, vm.ImageBytes);

                // **Update Data Pasien**
                pasien.TipePasien = vm.TipePasien;
                pasien.NoRekamMedisLama = vm.NoRekamMedisLama ?? pasien.NoRekamMedisLama;
                pasien.TitleId = vm.TitleId ?? pasien.TitleId;
                pasien.NamaLengkap = vm.NamaLengkap;
                pasien.IdentitasId = vm.IdentitasId;
                pasien.NoIdentitas = vm.NoIdentitas;
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
                pasien.PekerjaanId = vm.PekerjaanId ?? pasien.PekerjaanId;
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
                pasien.ImageBytes = vm.ImageBytes;


                pasien.UpdateBy = UserActiveId;
                pasien.UpdateDateTime = DateTimeOffset.Now;

                _applicationDbContext.PendaftaranPasienBarus.Update(pasien);
                _applicationDbContext.SaveChanges();

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                    qrCodeUrl = $"{Request.Scheme}://{Request.Host}{pasien.QrCode}",
                    uploadFotoUrl = $"{Request.Scheme}://{Request.Host}{pasien.FotoPath}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
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
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                pasien.DeleteBy = UserActiveId;
                pasien.DeleteDateTime = DateTimeOffset.Now;
                pasien.IsDelete = true;

                _applicationDbContext.PendaftaranPasienBarus.Update(pasien);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedPendaftaranPasienBaru(
        int page = 1,
        int perPage = 10,
        string? search = null,
        string? orderBy = "CreateDateTime",
        string? sortDirection = "desc",
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? startDate = null,
        [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
        DateTime? endDate = null,
        [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {
            // Query data
            var query = from a in _applicationDbContext.PendaftaranPasienBarus
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            PendaftaranPasienBaruId = a.PendaftaranPasienBaruId,
                            KodePasien = a.KodePasien,
                            NoRekamMedis = a.NoRekamMedis,
                            NamaLengkap = a.NamaLengkap,
                            JenisKelamin = a.JenisKelamin,
                            FotoName = a.FotoName,
                            ImageBytes = a.ImageBytes,
                            FotoPath = a.FotoPath,
                            NoRekamMedisLama = a.NoRekamMedisLama
                        };

            //Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    (u.KodePasien.Contains(search) || u.NamaLengkap.Contains(search) || u.NoRekamMedis.Contains(search) || u.NoRekamMedisLama.Contains(search))
                );
            }

            //Filter berdasarkan daterange
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(u =>
                    u.CreateDateTime.Date >= startDate.Value.Date &&
                    u.CreateDateTime.Date <= endDate.Value.Date
                );
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll) hanya jika periode memiliki nilai
            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-((int)today.DayOfWeek)) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-((int)today.DayOfWeek))
                        );
                        break;
                    case PeriodeFilter.ThisMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.LastMonth:
                        query = query.Where(u =>
                            u.CreateDateTime.Month == today.Month - 1 &&
                            u.CreateDateTime.Year == today.Year
                        );
                        break;
                    case PeriodeFilter.ThisYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year);
                        break;
                    case PeriodeFilter.LastYear:
                        query = query.Where(u => u.CreateDateTime.Year == today.Year - 1);
                        break;
                    case PeriodeFilter.Last3Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-3));
                        break;
                    case PeriodeFilter.Last6Months:
                        query = query.Where(u => u.CreateDateTime >= today.AddMonths(-6));
                        break;
                }
            }

            // Sorting Data dengan cara yang lebih aman
            query = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodePasien" => query.OrderByDescending(u => u.KodePasien),
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "NoRekamMedisLama" => query.OrderByDescending(u => u.NoRekamMedisLama),
                    "NamaLengkap" => query.OrderByDescending(u => u.NamaLengkap),
                    "JenisKelamin" => query.OrderByDescending(u => u.JenisKelamin),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodePasien" => query.OrderByDescending(u => u.KodePasien),
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "NoRekamMedisLama" => query.OrderByDescending(u => u.NoRekamMedisLama),
                    "NamaLengkap" => query.OrderByDescending(u => u.NamaLengkap),
                    "JenisKelamin" => query.OrderByDescending(u => u.JenisKelamin),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                };

            //Pagination
            var totalRows = query.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            var rows = query.Skip((page - 1) * perPage).Take(perPage).ToList();

            if (rows.Count == 0 && page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = rows,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }
    }
}
