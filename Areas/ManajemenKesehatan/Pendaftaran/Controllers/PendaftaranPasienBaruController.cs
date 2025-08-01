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
using System.Globalization;
using ZXing.QrCode.Internal;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using System.Net.Http.Headers;

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

        public static string HitungUmurLengkap(DateTime? tanggalLahir)
        {
            if (!tanggalLahir.HasValue) return "-";

            var today = DateTime.Today;
            int tahun = today.Year - tanggalLahir.Value.Year;
            int bulan = today.Month - tanggalLahir.Value.Month;
            int hari = today.Day - tanggalLahir.Value.Day;

            if (hari < 0)
            {
                bulan--;
                var prevMonth = today.AddMonths(-1);
                hari += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            }

            if (bulan < 0)
            {
                tahun--;
                bulan += 12;
            }

            return $"{tahun} tahun {bulan} bulan {hari} hari";
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
                            TipePasien = a.TipePasien,
                            NamaLengkap = a.NamaLengkap,
                            JenisKelamin = a.JenisKelamin,
                            FotoName = a.FotoName,
                            FotoPath = a.FotoPath,
                            TitleId = a.TitleId,
                            IdentitasId = a.IdentitasId,
                            NoIdentitas = a.NoIdentitas,
                            TempatLahir = a.TempatLahir,
                            TipePendaftaran = a.TipePendaftaran,
                            TanggalLahir = a.TanggalLahir.HasValue ? a.TanggalLahir.Value.ToString("yyyy-MM-dd") : null,
                            Umur = HitungUmurLengkap(a.TanggalLahir),
                            StatusPerkawinan = a.StatusPerkawinan,
                            AgamaId = a.AgamaId,
                            PendidikanTerakhirId = a.PendidikanTerakhirId,
                            AlamatIdentitas = a.AlamatIdentitas,
                            AlamatDomisili = a.AlamatDomisili,
                            NegaraId = a.NegaraId,
                            ProvinsiId = a.ProvinsiId,
                            KotaId = a.KotaId,
                            KecKabId = a.KecKabId,
                            KelurahanId = a.KelurahanId,
                            KodePos = a.KodePos,
                            Email = a.Email,
                            NoPasien = a.NoPasien,
                            NoWali2 = a.NoWali2,
                            NoWali3 = a.NoWali3,
                            NamaWali2 = a.NamaWali2,
                            NamaWali3 = a.NamaWali3,
                            Kewarganegaraan = a.Kewarganegaraan,
                            Suku = a.Suku,
                            StatusKewarganegaraan = a.StatusKewarganegaraan,
                            PekerjaanId = a.PekerjaanId,
                            NamaPerusahaan = a.NamaPerusahaan,
                            AlamatPerusahaan = a.AlamatPerusahaan,
                            NoTeleponPerusahaan = a.NoTeleponPerusahaan,
                            GolonganDarahId = a.GolonganDarahId,
                            Alergi = a.Alergi,
                            RiwayatPenyakit = a.RiwayatPenyakit,
                            RiwayatOperasi = a.RiwayatOperasi,
                            RiwayatPenyakitKeluarga = a.RiwayatPenyakitKeluarga,
                            HubunganKeluarga1 = a.HubunganKeluarga1,
                            HubunganPasien = a.HubunganPasien,
                            AlamatDarurat = a.AlamatDarurat,
                            NoTeleponDarurat = a.NoTeleponDarurat,
                            NamaOrangTua = a.NamaOrangTua,
                            IdentitasOrangTua = a.IdentitasOrangTua,
                            PekerjaanWali = a.PekerjaanWali,
                            HubunganKeluarga2 = a.HubunganKeluarga2,
                            HubunganKeluarga3 = a.HubunganKeluarga3,
                            NamaKontakDarurat = a.NamaKontakDarurat,
                            MembershipId = a.MembershipId,
                            imageUrl = !string.IsNullOrEmpty(a.FotoName)
                                        ? $"{Request.Scheme}://{Request.Host}/FotoPasienBaru/{a.FotoName}"
                                        : $"{Request.Scheme}://{Request.Host}/FotoPasienBaru/user.jpg",
                            QRUrl = $"{Request.Scheme}://{Request.Host}/QRCodePasienBaru/{Path.GetFileName(a.QrCode)}",
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
            var listdata = _applicationDbContext.PendaftaranPasienBarus
            .FirstOrDefault(p => p.PendaftaranPasienBaruId == id && !p.IsDelete);

            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }
            var parsed = listdata.TanggalLahir?.ToString("yyyy-MM-dd");

            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = new
                {
                    listdata.PendaftaranPasienBaruId,
                    listdata.KodePasien,
                    listdata.NoRekamMedis,
                    listdata.TipePasien,
                    listdata.TipePendaftaran,
                    listdata.TitleId,
                    listdata.NamaLengkap,
                    listdata.IdentitasId,
                    listdata.NoIdentitas,
                    listdata.TempatLahir,
                    TanggalLahir = parsed,
                    Umur = HitungUmurLengkap(listdata.TanggalLahir),
                    listdata.JenisKelamin,
                    listdata.StatusPerkawinan,
                    listdata.AgamaId,
                    listdata.PendidikanTerakhirId,
                    listdata.AlamatIdentitas,
                    listdata.AlamatDomisili,
                    listdata.NegaraId,
                    listdata.ProvinsiId,
                    listdata.KotaId,
                    listdata.KecKabId,
                    listdata.KelurahanId,
                    listdata.KodePos,
                    listdata.Email,
                    listdata.NoPasien,
                    listdata.NoWali2,
                    listdata.NoWali3,
                    listdata.NamaWali2,
                    listdata.NamaWali3,
                    listdata.Kewarganegaraan,
                    listdata.Suku,
                    listdata.StatusKewarganegaraan,
                    listdata.PekerjaanId,
                    listdata.NamaPerusahaan,
                    listdata.AlamatPerusahaan,
                    listdata.NoTeleponPerusahaan,
                    listdata.GolonganDarahId,
                    listdata.Alergi,
                    listdata.RiwayatPenyakit,
                    listdata.RiwayatOperasi,
                    listdata.RiwayatPenyakitKeluarga,
                    listdata.HubunganKeluarga1,
                    listdata.HubunganPasien,
                    listdata.AlamatDarurat,
                    listdata.NoTeleponDarurat,
                    listdata.NamaOrangTua,
                    listdata.IdentitasOrangTua,
                    listdata.PekerjaanWali,
                    listdata.NamaKontakDarurat,
                    listdata.HubunganKeluarga2,
                    listdata.HubunganKeluarga3,
                    listdata.FotoName,
                    listdata.FotoPath,
                    listdata.MembershipId,
                    imageUrl = !string.IsNullOrEmpty(listdata.FotoName)
                        ? $"{Request.Scheme}://{Request.Host}/FotoPasienBaru/{listdata.FotoName}"
                        : $"{Request.Scheme}://{Request.Host}/FotoPasienBaru/user.jpg",
                    QRUrl = $"{Request.Scheme}://{Request.Host}/QRCodePasienBaru/{Path.GetFileName(listdata.QrCode)}",
                }
            });
        }

        [HttpGet("nik/{nik}")]
        public IActionResult GetPendaftraanPasienBaruByNik(string nik)
        {
            var listdata = _applicationDbContext.PendaftaranPasienBarus
                .Where(p => p.NoIdentitas == nik && !p.IsDelete)
                .FirstOrDefault();

            if (listdata == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            var parsed = listdata.TanggalLahir?.ToString("yyyy-MM-dd");


            return Ok(new
            {
                message = "Ditemukan || 200 OK",
                data = new
                {
                    listdata.PendaftaranPasienBaruId,
                    listdata.KodePasien,
                    listdata.NoRekamMedis,
                    listdata.TipePasien,
                    listdata.TitleId,
                    listdata.NamaLengkap,
                    listdata.IdentitasId,
                    listdata.NoIdentitas,
                    listdata.TempatLahir,
                    TanggalLahir = parsed,
                    Umur = HitungUmurLengkap(listdata.TanggalLahir),
                    listdata.JenisKelamin,
                    listdata.StatusPerkawinan,
                    listdata.AgamaId,
                    listdata.PendidikanTerakhirId,
                    listdata.AlamatIdentitas,
                    listdata.AlamatDomisili,
                    listdata.NegaraId,
                    listdata.ProvinsiId,
                    listdata.KotaId,
                    listdata.KecKabId,
                    listdata.KelurahanId,
                    listdata.KodePos,
                    listdata.Email,
                    listdata.NoPasien,
                    listdata.NoWali2,
                    listdata.NoWali3,
                    listdata.NamaWali2,
                    listdata.NamaWali3,
                    listdata.Kewarganegaraan,
                    listdata.Suku,
                    listdata.StatusKewarganegaraan,
                    listdata.PekerjaanId,
                    listdata.NamaPerusahaan,
                    listdata.AlamatPerusahaan,
                    listdata.NoTeleponPerusahaan,
                    listdata.GolonganDarahId,
                    listdata.Alergi,
                    listdata.RiwayatPenyakit,
                    listdata.RiwayatOperasi,
                    listdata.RiwayatPenyakitKeluarga,
                    listdata.HubunganKeluarga1,
                    listdata.HubunganPasien,
                    listdata.AlamatDarurat,
                    listdata.NoTeleponDarurat,
                    listdata.NamaKontakDarurat,
                    listdata.NamaOrangTua,
                    listdata.IdentitasOrangTua,
                    listdata.PekerjaanWali,
                    listdata.HubunganKeluarga2,
                    listdata.HubunganKeluarga3,
                    listdata.FotoName,
                    listdata.FotoPath,
                    listdata.MembershipId,
                    imageUrl = !string.IsNullOrEmpty(listdata.FotoName)
                        ? $"{Request.Scheme}://{Request.Host}/FotoPasienBaru/{listdata.FotoName}"
                        : $"{Request.Scheme}://{Request.Host}/FotoPasienBaru/user.jpg",
                    QRUrl = $"{Request.Scheme}://{Request.Host}/QRCodePasienBaru/{Path.GetFileName(listdata.QrCode)}",
                }
            });
        }

        [HttpGet("get-image/{id}")]
        public async Task<IActionResult> GetImage(Guid id)
        {
            var fotoPath = _applicationDbContext.PendaftaranPasienBarus
                .Where(p => p.PendaftaranPasienBaruId == id)
                .Select(p => p.FotoPath)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(fotoPath))
            {
                return NotFound(new { message = "Foto tidak ditemukan." });
            }

            // Pastikan path lengkap menggunakan wwwroot
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, fotoPath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new { message = "File tidak ditemukan di server." });
            }

            var image = System.IO.File.OpenRead(fullPath);
            var contentType = GetContentType(fullPath);
            return File(image, contentType);
        }

        // Fungsi untuk mendapatkan MIME Type
        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" }
        };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        [HttpPost]
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

                var dateNow = DateTime.UtcNow; ;
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

                // Ambil semua NoRekamMedis yang dibuat hari ini
                var rekamMedisHariIni = _applicationDbContext.PendaftaranPasienBarus
                    .Where(p => p.CreateDateTime.Date == dateNow.Date)
                    .OrderByDescending(p => p.NoRekamMedis)
                    .ToList();

                int nextNumber = 1;

                if (rekamMedisHariIni.Any())
                {
                    var lastRekamMedis = rekamMedisHariIni.FirstOrDefault();
                    var lastNomorStr = lastRekamMedis.NoRekamMedis?.Split('-').LastOrDefault();

                    if (int.TryParse(lastNomorStr, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                // Buat nomor rekam medis baru
                string noRekamMedis = $"{kodeTahun}-{kodeHari}-{tipePasien}-{nextNumber:D2}";

                // Inisialisasi variabel untuk path dan filename QR code
                string QRPath = null;
                string qrCodeFileName = null;

                // 1. Lokasi logo (pastikan file ada di folder wwwroot/images)
                var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "logo.png");

                // 2. Generate QR code dengan logo asli sebagai byte[]
                var qrCodeBytes = QrCodeHelper.GenerateQrCodeWithLogoPngBytes(noRekamMedis, logoPath);

                // 3. Validasi folder tujuan penyimpanan QR code
                var uploadQrFolder = Path.Combine(_webHostEnvironment.WebRootPath, "QRCodePasienBaru");
                if (!Directory.Exists(uploadQrFolder))
                {
                    Directory.CreateDirectory(uploadQrFolder);
                }

                // 4. Tentukan nama file dan path penyimpanan
                qrCodeFileName = $"{noRekamMedis}.png";
                var qrCodeFilePath = Path.Combine(uploadQrFolder, qrCodeFileName);

                // 5. Simpan byte[] QR code ke dalam file menggunakan MemoryStream
                using (var memoryStream = new MemoryStream(qrCodeBytes))
                {
                    using (var stream = new FileStream(qrCodeFilePath, FileMode.Create))
                    {
                        memoryStream.CopyTo(stream); // Menyerupai vm.Foto.CopyTo()
                    }
                }

                // 6. Simpan path relatif ke database atau response
                QRPath = $"/QRCodePasienBaru/{qrCodeFileName}";

                // Upload QR ke server Flask setelah file sudah selesai ditulis
                using var clientQR = new HttpClient();

                using var qrUploadStream = new MemoryStream(qrCodeBytes); // langsung dari byte[], tidak dari file
                var qrContent = new MultipartFormDataContent {
                    {
                        new StreamContent(qrUploadStream)
                        {
                            Headers = { ContentType = new MediaTypeHeaderValue("image/png") }
                        },
                        "file", qrCodeFileName
                    },
                    { new StringContent("QRCodePasienBaru"), "folderTarget" }
                };

                var flaskResponseQR = await clientQR.PostAsync("http://160.20.104.98:5050/upload", qrContent);


                // Cek Duplikasi
                var isDuplicate = _applicationDbContext.PendaftaranPasienBarus
                    .Any(c => c.KodePasien == kodePasien && c.NoIdentitas == vm.NoIdentitas);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Terdapat duplikasi data! || 409 Conflict Data" });
                }

                // **Validasi & Simpan Foto Profil**
                string fotoPath = null;
                string fotoFileName = null;
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

                    fotoFileName = $"{kodePasien}{fileExtension}";
                    var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

                    using (var stream = new FileStream(fotoFilePath, FileMode.Create))
                    {
                        vm.Foto.CopyTo(stream);
                    }

                    fotoPath = $"/FotoPasienBaru/{fotoFileName}";

                    // 📤 **Kirim foto ke server Python Flask**
                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.Foto.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent {
                        // File utama
                        { new StreamContent(ms) {
                            Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Foto.ContentType) }
                        }, "file", fotoFileName },

                        // Nama folder tujuan di server Flask
                        { new StringContent("FotoPasienBaru"), "folderTarget" }
                    };

                    // Ganti IP di bawah dengan alamat Python Flask server Anda
                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                }
                else
                {
                    //Jika user tidak upload foto, gunakan foto default
                    fotoPath = "/FotoPasienBaru/user.jpg";
                    fotoFileName = "user.jpg";
                }

                // **Konversi `TanggalLahir` dari string "yyyy-MM-dd" ke `DateTime`**
                if (!DateTime.TryParseExact(vm.TanggalLahir, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });
                }
                parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);


                if (ModelState.IsValid)
                {
                    // Simpan Data
                    var daftar = new PendaftaranPasienBaru
                    {
                        PendaftaranPasienBaruId = Guid.NewGuid(),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        KodePasien = kodePasien,
                        NoRekamMedis = noRekamMedis,
                        TipePasien = vm.TipePasien,
                        TipePendaftaran = vm.TipePendaftaran,
                        TitleId = vm.TitleId,
                        NamaLengkap = vm.NamaLengkap,
                        IdentitasId = vm.IdentitasId,
                        NoIdentitas = vm.NoIdentitas,
                        TempatLahir = vm.TempatLahir,
                        TanggalLahir = parsedDate,
                        JenisKelamin = vm.JenisKelamin,
                        StatusPerkawinan = vm.StatusPerkawinan,
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
                        NoPasien = vm.NoPasien,
                        NoWali2 = vm.NoWali2,
                        NoWali3 = vm.NoWali3,
                        NamaWali2 = vm.NamaWali2,
                        NamaWali3 = vm.NamaWali3,
                        Kewarganegaraan = vm.Kewarganegaraan,
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
                        HubunganKeluarga1 = vm.HubunganKeluarga1,
                        HubunganPasien = vm.HubunganPasien,
                        NamaKontakDarurat = vm.NamaKontakDarurat,
                        AlamatDarurat = vm.AlamatDarurat,
                        NoTeleponDarurat = vm.NoTeleponDarurat,
                        NamaOrangTua = vm.NamaOrangTua,
                        IdentitasOrangTua = vm.IdentitasOrangTua,
                        PekerjaanWali = vm.PekerjaanWali,
                        HubunganKeluarga2 = vm.HubunganKeluarga2,
                        HubunganKeluarga3 = vm.HubunganKeluarga3,
                        MembershipId = vm.MembershipId,
                        FotoName = fotoFileName,
                        QrCode = QRPath, // Simpan hanya path QR Code
                        FotoPath = fotoPath,
                        //QrCodeImage = qrCodeBytes,

                    };
                    _applicationDbContext.PendaftaranPasienBarus.Add(daftar);
                    _applicationDbContext.SaveChanges();

                    return Created("", new
                    {
                        message = "Tambah Data Berhasil || 201 Created",
                        PasienBaruId = daftar.PendaftaranPasienBaruId,
                        NomorRekamMedis = daftar.NoRekamMedis,
                        qrCodeUrl = $"{Request.Scheme}://{Request.Host}/QRCodePasienBaru/{qrCodeFileName}",
                        url = $"{Request.Scheme}://{Request.Host}/FotoPasienBaru/{fotoFileName}"
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
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Konversi `TanggalLahir` dari string "yyyy-MM-dd" ke `DateTime`**
                if (!DateTime.TryParseExact(vm.TanggalLahir, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return BadRequest(new { message = "Format TanggalLahir tidak valid! Gunakan format yyyy-MM-dd." });

                }
                parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

                // **Update Data Pasien**
                pasien.TipePasien = vm.TipePasien;
                pasien.TipePendaftaran = vm.TipePendaftaran ?? pasien.TipePendaftaran;
                pasien.TitleId = vm.TitleId ?? pasien.TitleId;
                pasien.NamaLengkap = vm.NamaLengkap;
                pasien.IdentitasId = vm.IdentitasId;
                pasien.NoIdentitas = vm.NoIdentitas;
                pasien.TempatLahir = vm.TempatLahir ?? pasien.TempatLahir;
                pasien.TanggalLahir = vm.TanggalLahir != default ? parsedDate : pasien.TanggalLahir;
                pasien.JenisKelamin = vm.JenisKelamin ?? pasien.JenisKelamin;
                pasien.StatusPerkawinan = vm.StatusPerkawinan ?? pasien.StatusPerkawinan;
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
                pasien.NoPasien = vm.NoPasien ?? pasien.NoPasien;
                pasien.NoWali2 = vm.NoWali2 ?? pasien.NoWali2;
                pasien.NoWali3 = vm.NoWali3 ?? pasien.NoWali3;
                pasien.NamaWali2 = vm.NamaWali2 ?? pasien.NamaWali2;
                pasien.NamaWali3 = vm.NamaWali3 ?? pasien.NamaWali3;
                pasien.Kewarganegaraan = vm.Kewarganegaraan ?? pasien.Kewarganegaraan;
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
                pasien.HubunganKeluarga1 = vm.HubunganKeluarga1 ?? pasien.HubunganKeluarga1;
                pasien.HubunganPasien = vm.HubunganPasien ?? pasien.HubunganPasien;
                pasien.AlamatDarurat = vm.AlamatDarurat ?? pasien.AlamatDarurat;
                pasien.NoTeleponDarurat = vm.NoTeleponDarurat ?? pasien.NoTeleponDarurat;
                pasien.NamaKontakDarurat = vm.NamaKontakDarurat ?? pasien.NamaKontakDarurat;
                pasien.NamaOrangTua = vm.NamaOrangTua ?? pasien.NamaOrangTua;
                pasien.IdentitasOrangTua = vm.IdentitasOrangTua ?? pasien.IdentitasOrangTua;
                pasien.PekerjaanWali = vm.PekerjaanWali ?? pasien.PekerjaanWali;
                pasien.HubunganKeluarga2 = vm.HubunganKeluarga2 ?? pasien.HubunganKeluarga2;
                pasien.HubunganKeluarga3 = vm.HubunganKeluarga3 ?? pasien.HubunganKeluarga3;
                pasien.MembershipId = vm.MembershipId ?? pasien.MembershipId;

                // **Update Foto Profil Jika Ada**
                if (vm.Foto != null && vm.Foto.Length > 0)
                {
                    var maxSize = 2 * 1024 * 1024; // Maksimum 2MB
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

                    var fotoFileName = $"{pasien.KodePasien}{fileExtension}";
                    var oldFileName = pasien.FotoName ?? "";

                    using var client = new HttpClient();
                    using var ms = new MemoryStream();
                    await vm.Foto.CopyToAsync(ms);
                    ms.Position = 0;

                    var content = new MultipartFormDataContent
                    {
                        {
                            new StreamContent(ms)
                            {
                                Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Foto.ContentType) }
                            }, "file", fotoFileName
                        },
                        { new StringContent("FotoPasienBaru"), "folderTarget" },
                        { new StringContent(oldFileName), "oldFileName" }
                    };

                    var flaskResponse = await client.PostAsync("http://160.20.104.98:5050/upload", content);
                    if (!flaskResponse.IsSuccessStatusCode)
                    {
                        return StatusCode(500, new { message = "Gagal upload foto ke server Flask." });
                    }

                    pasien.FotoName = fotoFileName;
                    pasien.FotoPath = $"/FotoPasienBaru/{fotoFileName}"; // Simpan path relatif
                }

                pasien.UpdateBy = UserActiveId;
                pasien.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.PendaftaranPasienBarus.Update(pasien);
                _applicationDbContext.SaveChanges();

                return Ok(new
                {
                    message = "Update Data Berhasil || 200 OK",
                    qrCodeUrl = $"{Request.Scheme}://{Request.Host}/QRCodePasienBaru/{Path.GetFileName(pasien.QrCode)}",
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
                pasien.DeleteDateTime = DateTimeOffset.UtcNow;
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
                            TipePasien = a.TipePasien,
                            NamaLengkap = a.NamaLengkap,
                            JenisKelamin = a.JenisKelamin,
                            FotoName = a.FotoName,
                            FotoPath = a.FotoPath,
                            TitleId = a.TitleId,
                            IdentitasId = a.IdentitasId,
                            NoIdentitas = a.NoIdentitas,
                            TempatLahir = a.TempatLahir,
                            TipePendaftaran = a.TipePendaftaran,
                            TanggalLahir = a.TanggalLahir.HasValue ? a.TanggalLahir.Value.ToString("yyyy-MM-dd") : null,
                            Umur = HitungUmurLengkap(a.TanggalLahir),
                            StatusPerkawinan = a.StatusPerkawinan,
                            AgamaId = a.AgamaId,
                            PendidikanTerakhirId = a.PendidikanTerakhirId,
                            AlamatIdentitas = a.AlamatIdentitas,
                            AlamatDomisili = a.AlamatDomisili,
                            NegaraId = a.NegaraId,
                            ProvinsiId = a.ProvinsiId,
                            KotaId = a.KotaId,
                            KecKabId = a.KecKabId,
                            KelurahanId = a.KelurahanId,
                            KodePos = a.KodePos,
                            Email = a.Email,
                            NoPasien = a.NoPasien,
                            NoWali2 = a.NoWali2,
                            NoWali3 = a.NoWali3,
                            NamaWali2 = a.NamaWali2,
                            NamaWali3 = a.NamaWali3,
                            Kewarganegaraan = a.Kewarganegaraan,
                            Suku = a.Suku,
                            StatusKewarganegaraan = a.StatusKewarganegaraan,
                            PekerjaanId = a.PekerjaanId,
                            NamaPerusahaan = a.NamaPerusahaan,
                            AlamatPerusahaan = a.AlamatPerusahaan,
                            NoTeleponPerusahaan = a.NoTeleponPerusahaan,
                            GolonganDarahId = a.GolonganDarahId,
                            Alergi = a.Alergi,
                            RiwayatPenyakit = a.RiwayatPenyakit,
                            RiwayatOperasi = a.RiwayatOperasi,
                            RiwayatPenyakitKeluarga = a.RiwayatPenyakitKeluarga,
                            HubunganKeluarga1 = a.HubunganKeluarga1,
                            HubunganPasien = a.HubunganPasien,
                            AlamatDarurat = a.AlamatDarurat,
                            NoTeleponDarurat = a.NoTeleponDarurat,
                            NamaKontakDarurat = a.NamaKontakDarurat,
                            NamaOrangTua = a.NamaOrangTua,
                            IdentitasOrangTua = a.IdentitasOrangTua,
                            PekerjaanWali = a.PekerjaanWali,
                            HubunganKeluarga2 = a.HubunganKeluarga2,
                            HubunganKeluarga3 = a.HubunganKeluarga3,
                            MembershipId = a.MembershipId,
                            imageUrl = !string.IsNullOrEmpty(a.FotoName)
                                        ? $"{Request.Scheme}://{Request.Host}/FotoPasienBaru/{a.FotoName}"
                                        : $"{Request.Scheme}://{Request.Host}/FotoPasienBaru/user.jpg",
                            QRUrl = $"{Request.Scheme}://{Request.Host}/QRCodePasienBaru/{Path.GetFileName(a.QrCode)}",
                        };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
                query = query.Where(u =>
                    EF.Functions.ILike(u.NamaLengkap, search) ||
                    EF.Functions.ILike(u.KodePasien, search) ||
                    EF.Functions.ILike(u.NoRekamMedis, search) ||
                    EF.Functions.ILike(u.NoIdentitas, search)
                );
            }

            //// **Filter berdasarkan tanggal**
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
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
                    "NamaLengkap" => query.OrderByDescending(u => u.NamaLengkap),
                    "NoIdentitas" => query.OrderByDescending(u => u.NoIdentitas),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(u => u.CreateDateTime),
                    "CreateByName" => query.OrderByDescending(u => u.CreateByName),
                    "KodePasien" => query.OrderByDescending(u => u.KodePasien),
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "NamaLengkap" => query.OrderByDescending(u => u.NamaLengkap),
                    "NoIdentitas" => query.OrderByDescending(u => u.NoIdentitas),
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
