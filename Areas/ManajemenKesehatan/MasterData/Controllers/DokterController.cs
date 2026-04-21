using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Repositories;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using QuilvianSystemBackendDev.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;
using ZXing.QrCode.Internal;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using Microsoft.AspNetCore.Components.Routing;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class DokterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<DokterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _uploadUrl;

        public DokterController
            (ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DokterController> logger,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration
            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _uploadUrl = configuration["FileStorage:UploadUrl"];
        }

        // GET: api/Dokter
        [HttpGet]
        public async Task<IActionResult> GetAllDokter(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = _context.Dokters
                .Where(d => !d.IsDelete)
                .Select(d => new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = _context.UserActives
                        .Where(u => u.UserActiveId == d.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    d.DokterId,
                    d.KdDokter,
                    d.NmDokter,
                    d.Sip,
                    d.Str,
                    d.TglSip,
                    d.TglStr,
                    d.Nik,
                    d.Nohp,
                    d.Alamat,
                    d.Email,
                    d.UserActiveId,
                    d.IsAsuransi,
                    d.IsActive,
                    d.HargaVisit,
                    d.FotoName,
                    d.FotoPath,
                    imageUrl = !string.IsNullOrEmpty(d.FotoName)
                        ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
                        : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",
                    // Menambahkan daftar ID Asuransi
                    AsuransiIds = _context.DokterAsuransis
                        .Where(da => da.DokterId == d.DokterId)
                        .Select(da => da.AsuransiId)
                        .Distinct()
                        .ToList(),

                    NamaAsuransi = _context.DokterAsuransis
                        .Where(da => da.DokterId == d.DokterId)
                        .Join(_context.Asuransis, da => da.AsuransiId, a => a.AsuransiId, (da, a) => a.NamaAsuransi)
                        .Distinct()
                        .ToList(),

                    // Menambahkan daftar ID Poli
                    PoliIds = _context.DokterPolis
                        .Where(dp => dp.DokterId == d.DokterId)
                        .Select(dp => dp.PoliId)
                        .Distinct()
                        .ToList(),

                    NamaPoli = _context.DokterPolis
                        .Where(dp => dp.DokterId == d.DokterId)
                        .Join(_context.Polikliniks, dp => dp.PoliId, p => p.PoliklinikId, (dp, p) => p.NamaPoliklinik)
                        .Distinct()
                        .ToList(),

                    JadwalPraktek = (
                    from dp in _context.DokterPolis
                    join jp in _context.JadwalPrakteks on dp.DokterPoliId equals jp.DokterPoliId
                    join p in _context.Polikliniks on dp.PoliId equals p.PoliklinikId
                    where dp.DokterId == d.DokterId
                          && !dp.IsDelete
                          && !jp.IsDelete
                    select new
                    {
                        jp.JadwalPraktekId,
                        jp.HariPraktek,
                        jp.WaktuPraktek,
                        jp.JamMulai,
                        jp.JamBerakhir
                    })
                    .OrderBy(x => x.HariPraktek)
                    .ThenBy(x => x.JamMulai)
                    .ToList()


                })
                .ToList().OrderByDescending(a => a.CreateDateTime);

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

        // GET: api/Dokter/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDokterById(Guid id)
        {
            try
            {
                // =========================================================
                // 1) Dokter + CreateByName + Email (1 query)
                // =========================================================
                var dokterRow = await (
                    from d in _context.Dokters.AsNoTracking()
                    where !d.IsDelete && d.DokterId == id

                    join cb0 in _context.UserActives.AsNoTracking()
                        on d.CreateBy equals cb0.UserActiveId into cbJoin
                    from cb in cbJoin.DefaultIfEmpty()

                        // Ambil email dokter dari UserActiveId dokter (bukan FullName)
                    join ua0 in _context.UserActives.AsNoTracking()
                        on d.UserActiveId equals ua0.UserActiveId into uaJoin
                    from ua in uaJoin.DefaultIfEmpty()

                    select new
                    {
                        Dokter = d,
                        CreateByName = cb != null ? cb.FullName : null,
                        Email = ua != null ? ua.Email : null
                    }
                ).FirstOrDefaultAsync();

                if (dokterRow == null)
                    return NotFound(new { message = $"Dokter dengan ID {id} tidak ditemukan || 404 Not Found" });

                var dokter = dokterRow.Dokter;

                // =========================================================
                // 2) Asuransi Dokter (1 query)
                // =========================================================
                var asuransiList = await (
                    from da in _context.DokterAsuransis.AsNoTracking()
                    join a in _context.Asuransis.AsNoTracking()
                        on da.AsuransiId equals a.AsuransiId
                    where da.DokterId == id && !da.IsDelete
                    select new { da.AsuransiId, a.NamaAsuransi }
                ).Distinct().ToListAsync();

                var AsuransiIds = asuransiList.Select(x => x.AsuransiId).ToList();
                var NamaAsuransi = asuransiList.Select(x => x.NamaAsuransi).ToList();

                // =========================================================
                // 3) Poli + Jadwal (1 query)
                // =========================================================
                var poliJadwalRaw = await (
                    from dp in _context.DokterPolis.AsNoTracking()
                    join p in _context.Polikliniks.AsNoTracking()
                        on dp.PoliId equals p.PoliklinikId

                    // left join jadwal (karena bisa saja belum ada jadwal)
                    join jp0 in _context.JadwalPrakteks.AsNoTracking()
                        on dp.DokterPoliId equals jp0.DokterPoliId into jpJoin
                    from jp in jpJoin.DefaultIfEmpty()

                    where dp.DokterId == id && !dp.IsDelete
                    select new
                    {
                        dp.PoliId,
                        p.NamaPoliklinik,
                        dp.DokterPoliId,

                        Jadwal = jp == null || jp.IsDelete
                            ? null
                            : new
                            {
                                jp.JadwalPraktekId,
                                jp.HariPraktek,
                                jp.WaktuPraktek,
                                jp.JamMulai,
                                jp.JamBerakhir
                            }
                    }
                ).ToListAsync();

                var PoliIds = poliJadwalRaw
                    .Select(x => x.PoliId)
                    .Distinct()
                    .ToList();

                var NamaPoli = poliJadwalRaw
                    .Select(x => x.NamaPoliklinik)
                    .Where(x => x != null)
                    .Distinct()
                    .ToList();

                var JadwalPraktek = poliJadwalRaw
                    .Where(x => x.Jadwal != null)
                    .Select(x => x.Jadwal!)
                    .Distinct()
                    .OrderBy(j => j.HariPraktek)
                    .ThenBy(j => j.JamMulai)
                    .ToList();

                // =========================================================
                // 4) Image URL
                // =========================================================
                string imageUrl = !string.IsNullOrEmpty(dokter.FotoName)
                    ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{dokter.FotoName}"
                    : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg";

                // =========================================================
                // 5) Result
                // =========================================================
                var result = new
                {
                    dokter.CreateDateTime,
                    dokter.CreateBy,
                    dokterRow.CreateByName,

                    dokterRow.Email,

                    dokter.DokterId,
                    dokter.KdDokter,
                    dokter.NmDokter,
                    dokter.Spesialis,
                    dokter.Sip,
                    dokter.Str,
                    dokter.TglSip,
                    dokter.TglStr,
                    dokter.HargaVisit,
                    dokter.Nik,
                    dokter.Nohp,
                    dokter.Alamat,
                    dokter.IsAsuransi,
                    dokter.IsActive,
                    dokter.UserActiveId,
                    dokter.FotoName,
                    dokter.FotoPath,

                    imageUrl,

                    AsuransiIds,
                    NamaAsuransi,

                    PoliIds,
                    NamaPoli,

                    JadwalPraktek
                };

                return Ok(new
                {
                    message = "Berhasil mengambil data dokter || 200 OK",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        // GET: api/Dokter/by-email?email=xxx@xxx.com
        [HttpGet("by-email")]
        public async Task<IActionResult> GetDokterByEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { message = "Email wajib diisi." });

                email = email.Trim().ToLower();

                // =========================================================
                // 1) Dokter + CreateByName + Email (1 query)
                //    - Cari user active berdasarkan email
                //    - Ambil dokter berdasarkan Dokter.UserActiveId
                // =========================================================
                var dokterRow = await (
                    from ua in _context.UserActives.AsNoTracking()
                    where ua.Email != null && ua.Email.ToLower() == email

                    join d0 in _context.Dokters.AsNoTracking()
                        on ua.UserActiveId equals d0.UserActiveId
                    where !d0.IsDelete

                    join cb0 in _context.UserActives.AsNoTracking()
                        on d0.CreateBy equals cb0.UserActiveId into cbJoin
                    from cb in cbJoin.DefaultIfEmpty()

                    select new
                    {
                        Dokter = d0,
                        Email = ua.Email,
                        CreateByName = cb != null ? cb.FullName : null
                    }
                ).FirstOrDefaultAsync();

                if (dokterRow == null)
                {
                    return NotFound(new
                    {
                        message = $"Dokter dengan email {email} tidak ditemukan || 404 Not Found",
                        hint = "Pastikan Dokter.UserActiveId terisi dan terhubung ke UserActives.UserActiveId"
                    });
                }

                var dokter = dokterRow.Dokter;

                // =========================================================
                // 2) Asuransi Dokter (1 query)
                // =========================================================
                var asuransiList = await (
                    from da in _context.DokterAsuransis.AsNoTracking()
                    join a in _context.Asuransis.AsNoTracking()
                        on da.AsuransiId equals a.AsuransiId
                    where da.DokterId == dokter.DokterId && !da.IsDelete
                    select new { da.AsuransiId, a.NamaAsuransi }
                ).Distinct().ToListAsync();

                var AsuransiIds = asuransiList.Select(x => x.AsuransiId).ToList();
                var NamaAsuransi = asuransiList.Select(x => x.NamaAsuransi).ToList();

                // =========================================================
                // 3) Poli + Jadwal (1 query)
                // =========================================================
                var poliJadwalRaw = await (
                    from dp in _context.DokterPolis.AsNoTracking()
                    join p in _context.Polikliniks.AsNoTracking()
                        on dp.PoliId equals p.PoliklinikId

                    join jp0 in _context.JadwalPrakteks.AsNoTracking()
                        on dp.DokterPoliId equals jp0.DokterPoliId into jpJoin
                    from jp in jpJoin.DefaultIfEmpty()

                    where dp.DokterId == dokter.DokterId && !dp.IsDelete
                    select new
                    {
                        dp.PoliId,
                        p.NamaPoliklinik,
                        dp.DokterPoliId,
                        Jadwal = (jp == null || jp.IsDelete)
                            ? null
                            : new
                            {
                                jp.JadwalPraktekId,
                                jp.HariPraktek,
                                jp.WaktuPraktek,
                                jp.JamMulai,
                                jp.JamBerakhir
                            }
                    }
                ).ToListAsync();

                var PoliIds = poliJadwalRaw.Select(x => x.PoliId).Distinct().ToList();
                var NamaPoli = poliJadwalRaw.Select(x => x.NamaPoliklinik).Where(x => x != null).Distinct().ToList();

                var JadwalPraktek = poliJadwalRaw
                    .Where(x => x.Jadwal != null)
                    .Select(x => x.Jadwal!)
                    .Distinct()
                    .OrderBy(j => j.HariPraktek)
                    .ThenBy(j => j.JamMulai)
                    .ToList();

                // =========================================================
                // 4) Image URL
                // =========================================================
                string imageUrl = !string.IsNullOrEmpty(dokter.FotoName)
                    ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{dokter.FotoName}"
                    : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg";

                // =========================================================
                // 5) FINAL OBJECT
                // =========================================================
                var result = new
                {
                    dokter.CreateDateTime,
                    dokter.CreateBy,
                    dokterRow.CreateByName,

                    Email = dokterRow.Email,

                    dokter.DokterId,
                    dokter.KdDokter,
                    dokter.NmDokter,
                    dokter.Spesialis,
                    dokter.Sip,
                    dokter.Str,
                    dokter.TglSip,
                    dokter.TglStr,
                    dokter.Nik,
                    dokter.Nohp,
                    dokter.Alamat,
                    dokter.IsAsuransi,
                    dokter.IsActive,
                    dokter.UserActiveId,
                    dokter.FotoName,
                    dokter.FotoPath,

                    imageUrl,

                    AsuransiIds,
                    NamaAsuransi,

                    PoliIds,
                    NamaPoli,

                    JadwalPraktek
                };

                return Ok(new
                {
                    message = "Berhasil mengambil data dokter berdasarkan email || 200 OK",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpGet("get-image/{id}")]
        public async Task<IActionResult> GetImage(Guid id)
        {
            var fotoPath = _context.Dokters
                .Where(p => p.DokterId == id)
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

        // Ga dipake
        // POST: api/Dokter
        //[HttpPost]
        //public async Task<IActionResult> Create([FromForm] DokterViewModel vm)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //    {
        //        return BadRequest(new { message = "Data tidak valid." });
        //    }

        //    try
        //    {
        //        // **Ambil User ID dari JWT Claims**
        //        var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
        //        var UserActiveId = GetUserActive.UserActiveId;

        //        if (string.IsNullOrEmpty(EmailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        var dateNow = DateTime.UtcNow; ;
        //        var setDateNow = DateTimeOffset.UtcNow.ToString("yyMMdd");

        //        // Generate UserActiveCode
        //        var lastCode = _context.Dokters
        //            .Where(d => d.CreateDateTime.Date == dateNow.Date)
        //            .OrderByDescending(k => k.KdDokter)
        //            .FirstOrDefault();

        //        string kode;
        //        if (lastCode == null)
        //        {
        //            kode = $"DKR{setDateNow}0001";

        //        }
        //        else
        //        {
        //            var lastCodeTrim = lastCode.KdDokter.Substring(3, 6);
        //            if (lastCodeTrim != setDateNow)
        //            {
        //                kode = $"DKR{setDateNow}0001";
        //            }
        //            else
        //            {
        //                kode = $"DKR{setDateNow}" + (Convert.ToInt32(lastCode.KdDokter.Substring(9)) + 1).ToString("D4");
        //            }
        //        }


        //        // Cek Duplikasi
        //        var isDuplicate = await _context.Dokters
        //            .AnyAsync(c =>c.NmDokter.ToLower().Trim() == vm.NmDokter.ToLower().Trim() && c.IsDelete==false);

        //        // **Validasi & Simpan Foto Profil**
        //        string fotoPath = null;
        //        string fotoFileName = null;
        //        if (vm.Foto != null && vm.Foto.Length > 0)
        //        {
        //            var maxSize = 2 * 1024 * 1024;
        //            var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
        //            var fileExtension = Path.GetExtension(vm.Foto.FileName).ToLower();

        //            if (vm.Foto.Length > maxSize)
        //            {
        //                return BadRequest(new { message = "Ukuran file terlalu besar! Maksimum 2MB." });
        //            }

        //            if (!allowedExtensions.Contains(fileExtension))
        //            {
        //                return BadRequest(new { message = "Format file tidak valid! Gunakan JPG atau PNG." });
        //            }

        //            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "FotoDokter");
        //            if (!Directory.Exists(uploadFolder))
        //            {
        //                Directory.CreateDirectory(uploadFolder);
        //            }

        //            fotoFileName = $"{kode}{fileExtension}";
        //            var fotoFilePath = Path.Combine(uploadFolder, fotoFileName);

        //            using (var stream = new FileStream(fotoFilePath, FileMode.Create))
        //            {
        //                vm.Foto.CopyTo(stream);
        //            }

        //            fotoPath = $"/FotoDokter/{fotoFileName}";

        //            // 📤 **Kirim foto ke server Python Flask**
        //            using var client = new HttpClient();
        //            using var ms = new MemoryStream();
        //            await vm.Foto.CopyToAsync(ms);
        //            ms.Position = 0;

        //            var content = new MultipartFormDataContent {
        //                // File utama
        //                { new StreamContent(ms) {
        //                    Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(vm.Foto.ContentType) }
        //                }, "file", fotoFileName },

        //                // Nama folder tujuan di server Flask
        //                { new StringContent("FotoDokter"), "folderTarget" }
        //            };

        //            // Ganti IP di bawah dengan alamat Python Flask server Anda
        //            var flaskResponse = await client.PostAsync(_uploadUrl, content);

        //        }
        //        else
        //        {
        //            //Jika user tidak upload foto, gunakan foto default
        //            fotoPath = "/FotoDokter/dokter.jpg";
        //            fotoFileName = "dokter.jpg";
        //        }

        //        if (ModelState.IsValid)
        //        {
        //            var dokter = new Dokter
        //            {
        //                DokterId = Guid.NewGuid(),
        //                NmDokter = vm.NmDokter,
        //                Sip = vm.Sip,
        //                Str = vm.Str,
        //                TglSip = vm.TglSip,
        //                TglStr = vm.TglStr,
        //                FotoPath = fotoPath,
        //                FotoName = fotoFileName,
        //                Spesialis = vm.Spesialis,
        //                Nik = vm.Nik,
        //                KdDokter = kode,
        //                Email = vm.Email,
        //                Nohp = vm.Nohp,
        //                Alamat = vm.Alamat,
        //                HargaVisit = vm.HargaVisit,
        //                CreateDateTime = DateTimeOffset.UtcNow,
        //                CreateBy = UserActiveId,
        //                IsDelete = false,
        //                IsAsuransi = vm.IsAsuransi,
        //                IsActive = true,
        //            };
        //            _context.Dokters.Add(dokter);
        //            _context.SaveChanges();

        //            if (vm.AsuransiId != null && vm.AsuransiId.Any())
        //            {
        //                var dokterAsuransiList = vm.AsuransiId.Select(asuransiId => new DokterAsuransi
        //                {
        //                    DokterAsuransiId = Guid.NewGuid(),
        //                    DokterId = dokter.DokterId, // Gunakan ID dokter yang baru dibuat
        //                    AsuransiId = asuransiId, // Ambil ID asuransi dari list
        //                    CreateDateTime = DateTimeOffset.UtcNow,
        //                    CreateBy = UserActiveId,
        //                    IsDelete = false,
        //                }).ToList();

        //                _context.DokterAsuransis.AddRange(dokterAsuransiList);
        //                await _context.SaveChangesAsync();
        //            }

        //            if (vm.PoliId != null && vm.PoliId.Any())
        //            {
        //                var dokterPoliList = vm.PoliId.Select(poliId => new DokterPoli
        //                {
        //                    DokterPoliId = Guid.NewGuid(),
        //                    DokterId = dokter.DokterId, // Gunakan ID dokter yang baru dibuat
        //                    PoliId = poliId, // Ambil ID Poli dari list
        //                    CreateDateTime = DateTimeOffset.UtcNow,
        //                    CreateBy = UserActiveId,
        //                    IsDelete = false,
        //                }).ToList();

        //                _context.DokterPolis.AddRange(dokterPoliList);
        //                await _context.SaveChangesAsync();
        //            }

        //            return Created("", new
        //            {
        //                message = "Tambah Data Berhasil || 201 Created",
        //                //uploadFotoUrl = fotoPath != null ? $"{Request.Scheme}://{Request.Host}{fotoPath}" : null
        //            });

        //        }
        //        else
        //        {
        //            return BadRequest(new { message = "Data tidak valid !!! || 400 Bad Request" });
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}


        // PUT: api/Dokter/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] DokterViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Ambil User ID dari JWT Claims**
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // cari data
                var data = _context.Dokters.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // ✅ Update juga data di tabel UserActives
                var userActive = await _context.UserActives
                    .FirstOrDefaultAsync(u => u.FullName == data.NmDokter && u.Email == data.Email);

                //update data di tabel ApplicationUser
                var userLogin = await _userManager.FindByEmailAsync(data.Email.ToString());
                if (userLogin == null)
                {
                    return NotFound(new { message = "User tidak ditemukan." });
                }
                else
                {
                    userLogin.NamaUser = vm.NmDokter;
                    userLogin.Email = vm.Email;
                    userLogin.UserName = vm.Email;
                    userLogin.PhoneNumber = vm.Nohp;
                }
                var result = await _userManager.UpdateAsync(userLogin);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Email telah terdaftar, silakan gunakan email yang berbeda" });
                }

                //update data
                data.NmDokter = vm.NmDokter ?? data.NmDokter;
                data.Sip = vm.Sip ?? data.Sip;
                data.Str = vm.Str ?? data.Str;
                data.TglSip = vm.TglSip ?? data.TglSip;
                data.TglStr = vm.TglStr ?? data.TglStr;
                data.Nik = vm.Nik ?? data.Nik;
                data.Email = vm.Email ?? data.Email;
                data.Nohp = vm.Nohp ?? data.Nohp;
                data.Alamat = vm.Alamat ?? data.Alamat;
                data.Spesialis = vm.Spesialis ?? data.Spesialis;
                data.IsAsuransi = vm.IsAsuransi ?? data.IsAsuransi;
                data.HargaVisit = vm.HargaVisit ?? data.HargaVisit;

                if (userActive != null)
                {
                    userActive.FullName = vm.NmDokter ?? userActive.FullName;
                    userActive.IdentityNumber = vm.Nik ?? userActive.IdentityNumber;
                    userActive.Email = vm.Email ?? userActive.Email;
                    userActive.Address = vm.Alamat ?? userActive.Address;
                    userActive.Handphone = vm.Nohp ?? userActive.Handphone;

                    userActive.UpdateDateTime = DateTimeOffset.UtcNow;
                    userActive.UpdateBy = UserActiveId;

                    _context.UserActives.Update(userActive);
                }

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

                    var fotoFileName = $"{data.KdDokter}{fileExtension}";
                    var oldFileName = data.FotoName ?? "";

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
                        { new StringContent("FotoDokter"), "folderTarget" },
                        { new StringContent(oldFileName), "oldFileName" }
                    };

                    var flaskResponse = await client.PostAsync(_uploadUrl, content);
                    if (!flaskResponse.IsSuccessStatusCode)
                    {
                        return StatusCode(500, new { message = "Gagal upload foto ke server Flask." });
                    }

                    data.FotoName = fotoFileName;
                    data.FotoPath = $"/FotoDokter/{fotoFileName}";
                }

                data.UpdateDateTime = DateTimeOffset.UtcNow;
                data.UpdateBy = UserActiveId;

                _context.Dokters.Update(data);
                _context.SaveChanges();

                // **Asuransi**
                var asuransiLama = await _context.DokterAsuransis
                    .Where(da => da.DokterId == data.DokterId)
                    .ToListAsync();

                if (vm.AsuransiId == null || !vm.AsuransiId.Any())
                {
                    // Jika daftar asuransi baru kosong, hapus semua asuransi lama
                    _context.DokterAsuransis.RemoveRange(asuransiLama);
                }
                else
                {
                    // Hapus Asuransi Lama yang Tidak Ada dalam Daftar Baru
                    var asuransiYangDihapus = asuransiLama
                        .Where(da => !vm.AsuransiId.Contains(da.AsuransiId))
                        .ToList();

                    _context.DokterAsuransis.RemoveRange(asuransiYangDihapus);

                    // Tambahkan Asuransi Baru yang Belum Ada
                    var asuransiBaru = vm.AsuransiId
                        .Where(asuransiId => !asuransiLama.Any(da => da.AsuransiId == asuransiId))
                        .Select(asuransiId => new DokterAsuransi
                        {
                            DokterAsuransiId = Guid.NewGuid(),
                            DokterId = data.DokterId,
                            AsuransiId = asuransiId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = UserActiveId,
                            IsDelete = false
                        })
                        .ToList();

                    _context.DokterAsuransis.AddRange(asuransiBaru);
                }
                // **Poli**
                var poliLama = await _context.DokterPolis
                    .Where(dp => dp.DokterId == data.DokterId)
                    .ToListAsync();

                if (vm.PoliId == null || !vm.PoliId.Any())
                {
                    // Jika daftar poli baru kosong, hapus semua poli lama
                    _context.DokterPolis.RemoveRange(poliLama);
                }
                else
                {
                    // Hapus Poli Lama yang Tidak Ada dalam Daftar Baru
                    var poliYangDihapus = poliLama
                        .Where(dp => !vm.PoliId.Contains(dp.PoliId))
                        .ToList();

                    _context.DokterPolis.RemoveRange(poliYangDihapus);

                    // Tambahkan Poli Baru yang Belum Ada
                    var poliBaru = vm.PoliId
                        .Where(poliId => !poliLama.Any(dp => dp.PoliId == poliId))
                        .Select(poliId => new DokterPoli
                        {
                            DokterPoliId = Guid.NewGuid(),
                            DokterId = data.DokterId,
                            PoliId = poliId,
                            CreateDateTime = DateTimeOffset.UtcNow,
                            CreateBy = UserActiveId,
                            IsDelete = false
                        })
                        .ToList();

                    _context.DokterPolis.AddRange(poliBaru);
                }

                await _context.SaveChangesAsync();



                return Ok(new { message = "Data berhasil diupdate..." });
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        // DELETE: api/Dokter/{id}
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(Guid id)
        //{
        //    try
        //    {
        //        //Ambil User ID dari JWT Claims
        //        var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        var GetUserActive = _context.UserActives.Where(u => u.Email == EmailLogin).FirstOrDefault();
        //        var UserActiveId = GetUserActive.UserActiveId;

        //        if (string.IsNullOrEmpty(EmailLogin))
        //        {
        //            return Unauthorized(new { message = "User tidak terautentikasi!" });
        //        }

        //        // **Cari Data Dokter**
        //        var data = _context.Dokters.Find(id);
        //        if (data == null)
        //        {
        //            return NotFound(new { message = "Data tidak ditemukan." });
        //        }

        //        // cari data user di table user active
        //        var user = _context.UserActives
        //                .FirstOrDefault(u => u.FullName == data.NmDokter && u.Email == data.Email);
        //        if (user != null)
        //        {
        //            // Hapus data userdokter dari tabel Dokter
        //            user.IsDelete = true;
        //            user.IsActive = false;
        //            user.DeleteBy = UserActiveId;
        //            user.DeleteDateTime = DateTimeOffset.UtcNow;

        //            _context.UserActives.Update(user);
        //            await _context.SaveChangesAsync();
        //        }
        //        else
        //        {
        //            return NotFound(new { message = "User Active Dokter Ini Tidak Ditemukan" });
        //        }

        //        // Hapus user login dari tabel AspNetUsers (permanen)
        //        var userLogin = await _userManager.FindByEmailAsync(data.Email);
        //        if (userLogin != null)
        //        {
        //            var result = await _userManager.DeleteAsync(userLogin);
        //            if (!result.Succeeded)
        //            {
        //                return BadRequest(new { message = "Gagal menghapus akun login dari sistem." });
        //            }
        //        }
        //        // **Soft Delete (Tandai Data sebagai Terhapus)**
        //        data.DeleteBy = UserActiveId;
        //        data.DeleteDateTime = DateTimeOffset.UtcNow;
        //        data.IsDelete = true;
        //        data.IsActive = false;

        //        _context.Dokters.Update(data);
        //        _context.SaveChanges();

        //        return Ok(new { message = "Data berhasil dihapus..." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Ambil Email dari JWT Claims
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // Ambil User Active yang login
                var userActive = await _context.UserActives.FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (userActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan." });
                }

                var userActiveId = userActive.UserActiveId;

                // ===== 1. SOFT DELETE DATA DOKTER =====
                var dokter = await _context.Dokters.FindAsync(id);
                if (dokter == null)
                {
                    return NotFound(new { message = "Data Dokter tidak ditemukan." });
                }

                dokter.IsDelete = true;
                dokter.IsActive = false;
                dokter.DeleteBy = userActiveId;
                dokter.DeleteDateTime = DateTimeOffset.UtcNow;
                _context.Dokters.Update(dokter);

                // ===== 2. SOFT DELETE USERACTIVE DOKTER =====
                var user = await _context.UserActives
                    .FirstOrDefaultAsync(u => u.FullName == dokter.NmDokter && u.Email == dokter.Email);
                if (user != null)
                {
                    user.IsDelete = true;
                    user.IsActive = false;
                    user.DeleteBy = userActiveId;
                    user.DeleteDateTime = DateTimeOffset.UtcNow;
                    _context.UserActives.Update(user);
                }
                else
                {
                    return NotFound(new { message = "User Active Dokter ini tidak ditemukan." });
                }

                // ===== 3. DELETE USER LOGIN DARI ASPNETUSERS =====
                var userLogin = await _userManager.FindByEmailAsync(dokter.Email);
                if (userLogin != null)
                {
                    var result = await _userManager.DeleteAsync(userLogin);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { message = "Gagal menghapus akun login dari sistem." });
                    }
                }

                // ===== 4. SOFT DELETE DOKTER POLI =====
                var dokterPoli = await _context.DokterPolis.FindAsync(id);
                if (dokterPoli != null)
                {
                    dokterPoli.IsDelete = true;
                    dokterPoli.DeleteBy = userActiveId;
                    dokterPoli.DeleteDateTime = DateTimeOffset.UtcNow;
                    _context.DokterPolis.Update(dokterPoli);
                }

                // ===== 5. SOFT DELETE DOKTER ASURANSI =====
                var dokterAsuransi = await _context.DokterAsuransis.FindAsync(id);
                if (dokterAsuransi != null)
                {
                    dokterAsuransi.IsDelete = true;
                    dokterAsuransi.DeleteBy = userActiveId;
                    dokterAsuransi.DeleteDateTime = DateTimeOffset.UtcNow;
                    _context.DokterAsuransis.Update(dokterAsuransi);
                }

                // Simpan semua perubahan sekaligus
                await _context.SaveChangesAsync();

                return Ok(new { message = "Data berhasil dihapus." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        //[HttpGet("paged")]
        //public async Task<IActionResult> PagedDokter(
        //    int page = 1,
        //    int perPage = 10,
        //    string? search = null,
        //    Guid? AsuransiId = null,
        //    Guid? PoliId = null,
        //    string? orderBy = "CreateDateTime",
        //    string? sortDirection = "desc",
        //    [FromQuery] DateTime? startDate = null,
        //    [FromQuery] DateTime? endDate = null,
        //    [FromQuery] PeriodeFilter? periode = null)
        //{
        //    if (page < 1) page = 1;
        //    if (perPage < 1) perPage = 10;

        //    // 1) Base query: Memuat data dasar Dokter dengan eager loading
        //    // Ini akan memuat semua data terkait dalam satu kueri yang efisien.
        //    var baseQuery = _context.Dokters
        //        // Memuat tabel perantara DokterPolis dan data Poliklinik yang terkait
        //        .Include(d => d.DokterPolis)
        //            .ThenInclude(dp => dp.Poliklinik)
        //        // Memuat tabel perantara DokterAsuransis dan data Asuransi yang terkait
        //        .Include(d => d.DokterAsuransis)
        //            .ThenInclude(da => da.Asuransi)
        //        // Memuat data JadwalPraktek yang terkait
        //        .Include(d => d.JadwalPrakteks)
        //        .Where(d => !d.IsDelete);

        //    // 2) FILTER: by AsuransiId (GUID) dan PoliId (GUID)
        //    if (AsuransiId.HasValue)
        //    {
        //        baseQuery = baseQuery.Where(d => d.DokterAsuransis.Any(da => da.AsuransiId == AsuransiId.Value));
        //    }

        //    if (PoliId.HasValue)
        //    {
        //        baseQuery = baseQuery.Where(d => d.DokterPolis.Any(dp => dp.PoliId == PoliId.Value));
        //    }

        //    // 3) SEARCH: Mencari di berbagai kolom
        //    if (!string.IsNullOrWhiteSpace(search))
        //    {
        //        string q = $"%{search.ToLower()}%";
        //        baseQuery = baseQuery.Where(d =>
        //            EF.Functions.ILike(d.KdDokter, q) ||
        //            EF.Functions.ILike(d.NmDokter, q) ||
        //            EF.Functions.ILike(d.Email, q) ||
        //            // Memanfaatkan data yang sudah di-load dengan Include()
        //            d.DokterAsuransis.Any(da => EF.Functions.ILike(da.Asuransi.NamaAsuransi, q)) ||
        //            d.DokterPolis.Any(dp => EF.Functions.ILike(dp.Poliklinik.NamaPoliklinik, q))
        //        );
        //    }

        //    // 4) FILTER tanggal & periode
        //    if (startDate.HasValue && endDate.HasValue)
        //    {
        //        DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
        //        DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
        //        baseQuery = baseQuery.Where(d => d.CreateDateTime >= startUtc && d.CreateDateTime <= endUtc);
        //    }

        //    if (periode.HasValue)
        //    {
        //        DateTime today = DateTime.UtcNow.Date;
        //        switch (periode)
        //        {
        //            case PeriodeFilter.Today: baseQuery = baseQuery.Where(d => d.CreateDateTime.Date == today); break;
        //            case PeriodeFilter.ThisWeek:
        //                var weekStart = today.AddDays(-(int)today.DayOfWeek);
        //                baseQuery = baseQuery.Where(d => d.CreateDateTime.Date >= weekStart && d.CreateDateTime.Date <= today); break;
        //            case PeriodeFilter.LastWeek:
        //                var lastWeekStart = today.AddDays(-7 - (int)today.DayOfWeek);
        //                var lastWeekEnd = lastWeekStart.AddDays(6);
        //                baseQuery = baseQuery.Where(d => d.CreateDateTime.Date >= lastWeekStart && d.CreateDateTime.Date <= lastWeekEnd); break;
        //            case PeriodeFilter.ThisMonth: baseQuery = baseQuery.Where(d => d.CreateDateTime.Month == today.Month && d.CreateDateTime.Year == today.Year); break;
        //            case PeriodeFilter.LastMonth:
        //                var lastMonth = today.AddMonths(-1);
        //                baseQuery = baseQuery.Where(d => d.CreateDateTime.Month == lastMonth.Month && d.CreateDateTime.Year == lastMonth.Year); break;
        //            case PeriodeFilter.ThisYear: baseQuery = baseQuery.Where(d => d.CreateDateTime.Year == today.Year); break;
        //            case PeriodeFilter.LastYear: baseQuery = baseQuery.Where(d => d.CreateDateTime.Year == today.Year - 1); break;
        //            case PeriodeFilter.Last3Months: baseQuery = baseQuery.Where(d => d.CreateDateTime >= today.AddMonths(-3)); break;
        //            case PeriodeFilter.Last6Months: baseQuery = baseQuery.Where(d => d.CreateDateTime >= today.AddMonths(-6)); break;
        //        }
        //    }

        //    // 5) Hitung total baris
        //    var totalRows = await baseQuery.CountAsync();
        //    var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

        //    // Jika tidak ada data dan halaman yang diminta di luar batas, kembalikan NotFound.
        //    if (totalRows == 0 && page > 1)
        //    {
        //        return NotFound(new { message = "Page not found." });
        //    }

        //    // 6) SORTING
        //    var sortedQuery = sortDirection?.ToLower() == "desc"
        //        ? orderBy?.ToLower() switch
        //        {
        //            "createdatetime" => baseQuery.OrderByDescending(d => d.CreateDateTime),
        //            "kodedokter" => baseQuery.OrderByDescending(d => d.KdDokter),
        //            "namadokter" => baseQuery.OrderByDescending(d => d.NmDokter),
        //            "email" => baseQuery.OrderByDescending(d => d.Email),
        //            _ => baseQuery.OrderByDescending(d => d.CreateDateTime)
        //        }
        //        : orderBy?.ToLower() switch
        //        {
        //            "createdatetime" => baseQuery.OrderBy(d => d.CreateDateTime),
        //            "kodedokter" => baseQuery.OrderBy(d => d.KdDokter),
        //            "namadokter" => baseQuery.OrderBy(d => d.NmDokter),
        //            "email" => baseQuery.OrderBy(d => d.Email),
        //            _ => baseQuery.OrderBy(d => d.CreateDateTime)
        //        };

        //    // 7) PAGINATION
        //    var rows = await sortedQuery.Skip((page - 1) * perPage).Take(perPage).ToListAsync();

        //    // 8) PROJECTION AKHIR
        //    // Menggunakan data yang sudah di-load dengan Include()
        //    var projectedRows = rows.Select(d => new
        //    {
        //        d.CreateDateTime,
        //        d.CreateBy,
        //        CreateByName = _context.UserActives.FirstOrDefault(u => u.UserActiveId == d.CreateBy)?.FullName ?? "-",
        //        d.DokterId,
        //        d.KdDokter,
        //        d.NmDokter,
        //        d.Sip,
        //        d.Str,
        //        d.TglSip,
        //        d.TglStr,
        //        d.Spesialis,
        //        d.Nik,
        //        d.Nohp,
        //        d.Alamat,
        //        d.Email,
        //        d.UserActiveId,
        //        d.IsAsuransi,
        //        d.IsActive,
        //        d.FotoName,
        //        d.FotoPath,
        //        imageUrl = !string.IsNullOrEmpty(d.FotoName)
        //            ? $"{Request.Scheme}://{Request.Host}/FotoDokter/{d.FotoName}"
        //            : $"{Request.Scheme}://{Request.Host}/FotoDokter/dokter.jpg",

        //        AsuransiIds = d.DokterAsuransis.Select(da => da.AsuransiId).ToList(),
        //        NamaAsuransi = d.DokterAsuransis.Select(da => da.Asuransi.NamaAsuransi).ToList(),
        //        PoliIds = d.DokterPolis.Select(dp => dp.PoliId).ToList(),
        //        NamaPoli = d.DokterPolis.Select(dp => dp.Poliklinik.NamaPoliklinik).ToList(),
        //        JadwalPraktek = d.JadwalPrakteks
        //            .Where(jp => !jp.IsDelete)
        //            .Select(jp => new
        //            {
        //                jp.JadwalPraktekId,
        //                jp.HariPraktek,
        //                jp.WaktuPraktek,
        //                jp.JamMulai,
        //                jp.JamBerakhir
        //            })
        //            .OrderBy(x => x.HariPraktek)
        //            .ThenBy(x => x.JamMulai)
        //            .ToList()
        //    }).ToList();

        //    return Ok(new
        //    {
        //        status = "success",
        //        message = "Data retrieved successfully",
        //        data = new
        //        {
        //            Rows = projectedRows,
        //            TotalRows = totalRows,
        //            CurrentPage = page,
        //            PerPage = perPage,
        //            TotalPages = totalPages
        //        }
        //    });
        //}

        [HttpGet("paged")]
        public async Task<IActionResult> PagedDokter(
            int page = 1,
            int perPage = 10,
            string? search = null,
            Guid? asuransiId = null,
            Guid? poliId = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            if (perPage > 100) perPage = 100; // biar swagger ga “meledak”

            // =========================================
            // 1) BASE QUERY (Dokter saja, ringan)
            // =========================================
            IQueryable<Dokter> baseQuery = _context.Dokters
                .AsNoTracking()
                .Where(d => !d.IsDelete);

            // Filter AsuransiId via EXISTS (Any)
            if (asuransiId.HasValue && asuransiId.Value != Guid.Empty)
            {
                var asu = asuransiId.Value;
                baseQuery = baseQuery.Where(d =>
                    _context.DokterAsuransis.AsNoTracking().Any(da =>
                        !da.IsDelete && da.DokterId == d.DokterId && da.AsuransiId == asu));
            }

            // Filter PoliId via EXISTS (Any)
            if (poliId.HasValue && poliId.Value != Guid.Empty)
            {
                var poli = poliId.Value;
                baseQuery = baseQuery.Where(d =>
                    _context.DokterPolis.AsNoTracking().Any(dp =>
                        !dp.IsDelete && dp.DokterId == d.DokterId && dp.PoliId == poli));
            }

            // Search (kode/nama/email + nama asuransi/poli) pakai EXISTS, bukan join besar
            if (!string.IsNullOrWhiteSpace(search))
            {
                var like = $"%{search.Trim()}%";

                baseQuery = baseQuery.Where(d =>
                    (d.KdDokter != null && EF.Functions.ILike(d.KdDokter, like)) ||
                    (d.NmDokter != null && EF.Functions.ILike(d.NmDokter, like)) ||
                    (d.Email != null && EF.Functions.ILike(d.Email, like)) ||

                    // exists nama asuransi
                    (from da in _context.DokterAsuransis.AsNoTracking()
                     join a in _context.Asuransis.AsNoTracking() on da.AsuransiId equals a.AsuransiId
                     where !da.IsDelete && da.DokterId == d.DokterId
                           && a.NamaAsuransi != null
                           && EF.Functions.ILike(a.NamaAsuransi, like)
                     select 1).Any() ||

                    // exists nama poli
                    (from dp in _context.DokterPolis.AsNoTracking()
                     join p in _context.Polikliniks.AsNoTracking() on dp.PoliId equals p.PoliklinikId
                     where !dp.IsDelete && dp.DokterId == d.DokterId
                           && p.NamaPoliklinik != null
                           && EF.Functions.ILike(p.NamaPoliklinik, like)
                     select 1).Any()
                );
            }

            // Filter tanggal (lebih sargable: < endExclusive)
            if (startDate.HasValue && endDate.HasValue)
            {
                var start = startDate.Value.Date.ToUniversalTime();
                var endExclusive = endDate.Value.Date.AddDays(1).ToUniversalTime();
                baseQuery = baseQuery.Where(d => d.CreateDateTime >= start && d.CreateDateTime < endExclusive);
            }

            // Filter periode (usahakan range, bukan .Date)
            if (periode.HasValue)
            {
                var today = DateTime.UtcNow.Date;
                DateTime start;
                DateTime endExclusive;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        start = today;
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Yesterday:
                        start = today.AddDays(-1);
                        endExclusive = today;
                        break;

                    case PeriodeFilter.ThisWeek:
                        start = today.AddDays(-(int)today.DayOfWeek);
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.LastWeek:
                        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
                        start = thisWeekStart.AddDays(-7);
                        endExclusive = thisWeekStart;
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTime(today.Year, today.Month, 1);
                        endExclusive = start.AddMonths(1);
                        break;

                    case PeriodeFilter.LastMonth:
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
                        start = thisMonthStart.AddMonths(-1);
                        endExclusive = thisMonthStart;
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTime(today.Year, 1, 1);
                        endExclusive = start.AddYears(1);
                        break;

                    case PeriodeFilter.LastYear:
                        start = new DateTime(today.Year - 1, 1, 1);
                        endExclusive = start.AddYears(1);
                        break;

                    case PeriodeFilter.Last3Months:
                        start = today.AddMonths(-3);
                        endExclusive = today.AddDays(1);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = today.AddMonths(-6);
                        endExclusive = today.AddDays(1);
                        break;

                    default:
                        start = DateTime.MinValue;
                        endExclusive = DateTime.MaxValue;
                        break;
                }

                baseQuery = baseQuery.Where(d => d.CreateDateTime >= start && d.CreateDateTime < endExclusive);
            }

            // =========================================
            // 2) COUNT (hanya baseQuery, ringan)
            // =========================================
            var totalRows = await baseQuery.CountAsync(ct);
            if (totalRows == 0)
                return NotFound(new { message = "Data tidak ditemukan." });

            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);
            if (page > totalPages)
                return NotFound(new { message = "Page not found." });

            // =========================================
            // 3) SORTING (di baseQuery)
            //    Catatan: sort CreateByName butuh join, jadi saya support basic saja
            // =========================================
            bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var order = (orderBy ?? "CreateDateTime").Trim().ToLowerInvariant();

            IQueryable<Dokter> sortedQuery = order switch
            {
                "kddokter" => desc ? baseQuery.OrderByDescending(x => x.KdDokter) : baseQuery.OrderBy(x => x.KdDokter),
                "nmdokter" => desc ? baseQuery.OrderByDescending(x => x.NmDokter) : baseQuery.OrderBy(x => x.NmDokter),
                "email" => desc ? baseQuery.OrderByDescending(x => x.Email) : baseQuery.OrderBy(x => x.Email),
                _ => desc ? baseQuery.OrderByDescending(x => x.CreateDateTime) : baseQuery.OrderBy(x => x.CreateDateTime),
            };

            // =========================================
            // 4) PAGE IDS dulu (super ringan)
            // =========================================
            var pagedDokterIds = await sortedQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(d => d.DokterId)
                .ToListAsync(ct);

            if (pagedDokterIds.Count == 0)
                return NotFound(new { message = "Data tidak ditemukan." });

            var idSet = pagedDokterIds.ToHashSet();

            // =========================================
            // 5) LOAD DOKTER + CreateByName (1 query)
            // =========================================
            var dokterRows = await (
                from d in _context.Dokters.AsNoTracking()
                where idSet.Contains(d.DokterId)

                join u0 in _context.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into uj
                from u in uj.DefaultIfEmpty()

                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    d.DokterId,
                    d.KdDokter,
                    d.NmDokter,
                    d.Sip,
                    d.Str,
                    d.TglSip,
                    d.TglStr,
                    d.Spesialis,
                    d.Nik,
                    d.Nohp,
                    d.Alamat,
                    d.Email,
                    d.UserActiveId,
                    d.IsAsuransi,
                    d.IsActive,
                    d.HargaVisit,
                    d.FotoName,
                    d.FotoPath
                }
            ).ToListAsync(ct);

            var dokterDict = dokterRows.ToDictionary(x => x.DokterId, x => x);
            var dokterPaged = pagedDokterIds.Where(dokterDict.ContainsKey).Select(id => dokterDict[id]).ToList();

            // =========================================
            // 6) LOAD ASURANSI (1 query)
            // =========================================
            var asuransiData = await (
                from da in _context.DokterAsuransis.AsNoTracking()
                join a in _context.Asuransis.AsNoTracking()
                    on da.AsuransiId equals a.AsuransiId
                where !da.IsDelete && idSet.Contains(da.DokterId)
                select new { da.DokterId, da.AsuransiId, a.NamaAsuransi }
            ).ToListAsync(ct);

            // =========================================
            // 7) LOAD POLI (1 query)
            // =========================================
            var poliData = await (
                from dp in _context.DokterPolis.AsNoTracking()
                join p in _context.Polikliniks.AsNoTracking()
                    on dp.PoliId equals p.PoliklinikId
                where !dp.IsDelete && idSet.Contains(dp.DokterId)
                select new { dp.DokterId, dp.DokterPoliId, dp.PoliId, p.NamaPoliklinik }
            ).ToListAsync(ct);

            // =========================================
            // 8) LOAD JADWAL (1 query)
            // =========================================
            var jadwalData = await (
                from dp in _context.DokterPolis.AsNoTracking()
                join jp in _context.JadwalPrakteks.AsNoTracking()
                    on dp.DokterPoliId equals jp.DokterPoliId
                where !dp.IsDelete && !jp.IsDelete && idSet.Contains(dp.DokterId)
                select new
                {
                    dp.DokterId,
                    jp.JadwalPraktekId,
                    jp.HariPraktek,
                    jp.WaktuPraktek,
                    jp.JamMulai,
                    jp.JamBerakhir
                }
            ).ToListAsync(ct);

            // =========================================
            // 9) LOOKUP + BUILD RESULT
            // =========================================
            var asuransiLookup = asuransiData.ToLookup(x => x.DokterId);
            var poliLookup = poliData.ToLookup(x => x.DokterId);
            var jadwalLookup = jadwalData.ToLookup(x => x.DokterId);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var rows = dokterPaged.Select(d =>
            {
                var asuItems = asuransiLookup[d.DokterId].ToList();
                var poliItems = poliLookup[d.DokterId].ToList();
                var jadwalItems = jadwalLookup[d.DokterId]
                    .OrderBy(x => x.HariPraktek)
                    .ThenBy(x => x.JamMulai)
                    .Select(x => new
                    {
                        x.JadwalPraktekId,
                        x.HariPraktek,
                        x.WaktuPraktek,
                        x.JamMulai,
                        x.JamBerakhir
                    })
                    .ToList();

                var imageUrl = !string.IsNullOrEmpty(d.FotoName)
                    ? $"{baseUrl}/FotoDokter/{d.FotoName}"
                    : $"{baseUrl}/FotoDokter/dokter.jpg";

                return new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    d.CreateByName,

                    d.DokterId,
                    d.KdDokter,
                    d.NmDokter,
                    d.Sip,
                    d.Str,
                    d.TglSip,
                    d.TglStr,
                    d.Spesialis,
                    d.Nik,
                    d.Nohp,
                    d.Alamat,
                    d.Email,
                    d.UserActiveId,
                    d.IsAsuransi,
                    d.IsActive,
                    d.HargaVisit,
                    d.FotoName,
                    d.FotoPath,

                    imageUrl,

                    AsuransiIds = asuItems.Select(x => x.AsuransiId).Distinct().ToList(),
                    NamaAsuransi = asuItems.Select(x => x.NamaAsuransi).Where(x => x != null).Distinct().ToList(),

                    PoliIds = poliItems.Select(x => x.PoliId).Distinct().ToList(),
                    NamaPoli = poliItems.Select(x => x.NamaPoliklinik).Where(x => x != null).Distinct().ToList(),

                    JadwalPraktek = jadwalItems
                };
            }).ToList();

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
