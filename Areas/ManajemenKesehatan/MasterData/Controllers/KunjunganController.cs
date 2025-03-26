using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class KunjunganController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<KunjunganController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KunjunganController
        (
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,

            ILogger<KunjunganController> logger,
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
        public async Task<IActionResult> GetAllKunjungan(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;
            // Query data
            var result = from a in _applicationDbContext.Kunjungans
                        join u in _applicationDbContext.UserActives
                        on a.CreateBy equals u.UserActiveId
                        where a.IsDelete == false
                        select new
                        {
                            CreateDateTime = a.CreateDateTime,
                            CreateBy = a.CreateBy,
                            CreateByName = u.FullName,
                            FullName = u.FullName,
                            KunjunganId = a.KunjunganID,
                            AsuransiId = a.AsuransiId,
                            PoliklinikId = a.PoliklinikId,
                            DokterId = a.DokterId,
                            TindakanId = a.TindakanId,
                            PasienId = a.PasienId,
                            NoRekamMedis = a.NoRekamMedis,
                            TipePasien = a.TipePasien,
                            TipePembayaran = a.TipePembayaran,
                            Antrian = a.Antrian,
                            JumlahKunjungan = a.JumlahKunjungan,
                        };

            // Hitung total data sebelum paginasi
            var totalRows = result.Count();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            // Ambil data sesuai paging
            var listdata = result
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
        public async Task<IActionResult> GetAsuransiPasienById(Guid id)
        {
            var listdata = _applicationDbContext.Kunjungans.Find(id);
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

        [HttpPost]
        public async Task<IActionResult> CreateAsuransiPasien([FromBody] KunjunganViewModel request)
        {
            if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
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

                // Periksa apakah pasien dan asuransi ada di database
                //var pasienExists = _applicationDbContext.PendaftaranPasienBarus
                //                      .Any(p => p.PendaftaranPasienBaruId.ToString() == request.PasienId);

                //var asuransiExists = _applicationDbContext.Asuransis
                //                      .Any(a => a.AsuransiId.ToString() == request.AsuransiId);

                //if (!pasienExists || !asuransiExists)
                //{
                //    return NotFound(new { message = "Pasien atau Asuransi tidak ditemukan!" });
                //}

                // Generate Urutan Kunjungan

                // 🔹 Validasi tipe pasien (harus "Rujukan" atau "Umum")
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });
                }

                // 🔹 Ambil antrian terakhir berdasarkan tipe pasien
                var lastAntrian = _applicationDbContext.Kunjungans
                    .Where(a => a.TipePasien == request.TipePasien)
                    .OrderByDescending(a => a.CreateDateTime)
                    .FirstOrDefault();

                // 🔹 Reset antrian jika hari berganti
                bool isNewDay = lastAntrian == null || lastAntrian.CreateDateTime.UtcDateTime.Date != DateTime.UtcNow.Date;
                int nextNumber = isNewDay ? 1 : int.Parse(lastAntrian.Antrian.Split('-')[1]) + 1;

                // Format nomor antrian
                string prefix = request.TipePasien.Equals("Rujukan", StringComparison.OrdinalIgnoreCase) ? "R" : "U";
                string nomorAntrian = $"{prefix}-{nextNumber:D3}";

                // 🔹 Validasi jenis kunjungan (hanya "Rawat Inap" atau "Rawat Jalan")
                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(request.JenisKunjungan, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });
                }

                // 🔹 Cek apakah pasien sudah memiliki riwayat kunjungan
                var kunjunganPasien = _applicationDbContext.Kunjungans
                    .FirstOrDefault(k => k.PasienId == request.PasienId);

                // 🔹 Jika belum ada riwayat, buat baru
                if (kunjunganPasien == null)
                {
                    var kunjunganBaru = new Kunjungan
                    {
                        KunjunganID = Guid.NewGuid(),
                        PasienId = request.PasienId,
                        DokterId = request.DokterId,
                        PoliklinikId = request.PoliklinikId,
                        TindakanId = request.TindakanId,
                        AsuransiId = request.AsuransiId,
                        JumlahKunjungan = JsonSerializer.Serialize(new List<string> { $"{(request.JenisKunjungan == "Rawat Inap" ? "IP" : "OP")}-1" }),
                        CreateDateTime = DateTimeOffset.UtcNow,
                        CreateBy = UserActiveId,
                        NoRekamMedis = request.NoRekamMedis,
                        TipePasien = request.TipePasien,
                        TipePembayaran = request.TipePembayaran,
                        Antrian = nomorAntrian,
                        IsDelete = false
                    };

                    _applicationDbContext.Kunjungans.Add(kunjunganBaru);
                }
                else
                {
                    // 🔹 Jika sudah ada, update jumlah kunjungan
                    var jumlahKunjungan = JsonSerializer.Deserialize<List<string>>(kunjunganPasien.JumlahKunjungan) ?? new List<string>();

                    string jk = request.JenisKunjungan == "Rawat Inap" ? "IP" : "OP";
                    var existing = jumlahKunjungan.FirstOrDefault(j => j.StartsWith(jk));

                    if (existing != null)
                    {
                        // Update jumlah kunjungan
                        int currentCount = int.Parse(existing.Split('-')[1]) + 1;
                        jumlahKunjungan[jumlahKunjungan.IndexOf(existing)] = $"{jk}-{currentCount}";
                    }
                    else
                    {
                        // Tambahkan kunjungan baru
                        jumlahKunjungan.Add($"{jk}-1");
                    }

                    // Simpan perubahan
                    kunjunganPasien.JumlahKunjungan = JsonSerializer.Serialize(jumlahKunjungan);
                    _applicationDbContext.Kunjungans.Update(kunjunganPasien);
                }

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "200 || Kunjungan berhasil ditambahkan!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsuransiPasien(Guid id, [FromBody] KunjunganViewModel request)
        {
            if (request == null || !request.PasienId.HasValue || request.PasienId == Guid.Empty)
            {
                return BadRequest(new { message = "Data tidak boleh kosong!" });
            }

            try
            {
                // Ambil user login
                var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var GetUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
                var UserActiveId = GetUserActive.UserActiveId;

                if (string.IsNullOrEmpty(EmailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                // Ambil data kunjungan berdasarkan id
                var kunjunganPasien = await _applicationDbContext.Kunjungans.FindAsync(id);
                if (kunjunganPasien == null)
                {
                    return NotFound(new { message = "Data kunjungan tidak ditemukan!" });
                }

                // Validasi TipePasien
                if (!new[] { "Rujukan", "Umum" }.Contains(request.TipePasien, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Tipe pasien tidak valid. Gunakan hanya 'Rujukan' atau 'Umum'." });
                }

                // Validasi JenisKunjungan
                if (!new[] { "Rawat Inap", "Rawat Jalan" }.Contains(request.JenisKunjungan, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Jenis kunjungan tidak valid. Gunakan hanya 'Rawat Inap' atau 'Rawat Jalan'." });
                }

                // Update jumlah kunjungan
                var jumlahKunjungan = JsonSerializer.Deserialize<List<string>>(kunjunganPasien.JumlahKunjungan) ?? new List<string>();
                string jk = request.JenisKunjungan == "Rawat Inap" ? "IP" : "OP";
                var existing = jumlahKunjungan.FirstOrDefault(j => j.StartsWith(jk));

                if (existing != null)
                {
                    int currentCount = int.Parse(existing.Split('-')[1]) + 1;
                    jumlahKunjungan[jumlahKunjungan.IndexOf(existing)] = $"{jk}-{currentCount}";
                }
                else
                {
                    jumlahKunjungan.Add($"{jk}-1");
                }

                // Update properti lainnya
                kunjunganPasien.DokterId = request.DokterId;
                kunjunganPasien.PoliklinikId = request.PoliklinikId;
                kunjunganPasien.TindakanId = request.TindakanId;
                kunjunganPasien.AsuransiId = request.AsuransiId;
                kunjunganPasien.NoRekamMedis = request.NoRekamMedis;
                kunjunganPasien.TipePasien = request.TipePasien;
                kunjunganPasien.TipePembayaran = request.TipePembayaran;
                kunjunganPasien.JumlahKunjungan = JsonSerializer.Serialize(jumlahKunjungan);
                kunjunganPasien.UpdateDateTime = DateTimeOffset.UtcNow;
                kunjunganPasien.UpdateBy = UserActiveId;

                _applicationDbContext.Kunjungans.Update(kunjunganPasien);
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new { message = "200 || Data kunjungan berhasil diperbarui!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
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
                var data = _applicationDbContext.Kunjungans.Find(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = UserActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;
                data.IsDelete = true;

                _applicationDbContext.Kunjungans.Update(data);
                _applicationDbContext.SaveChanges();

                return Ok(new { message = "Data berhasil dihapus..." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public IActionResult PagedTitle(
            int page = 1,
            int perPage = 10,
            string? search = null,
            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,
            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,
            [FromQuery] PeriodeFilter? periode = null)
        {
            // Query data
            var query = from a in _applicationDbContext.Kunjungans
                         join u in _applicationDbContext.UserActives
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false
                         select new
                         {
                             CreateDateTime = a.CreateDateTime,
                             CreateBy = a.CreateBy,
                             CreateByName = u.FullName,
                             FullName = u.FullName,
                             KunjunganId = a.KunjunganID,
                             AsuransiId = a.AsuransiId,
                             PoliklinikId = a.PoliklinikId,
                             DokterId = a.DokterId,
                             TindakanId = a.TindakanId,
                             PasienId = a.PasienId,
                             NoRekamMedis = a.NoRekamMedis,
                             TipePasien = a.TipePasien,
                             TipePembayaran = a.TipePembayaran,
                             Antrian = a.Antrian,
                             JumlahKunjungan = a.JumlahKunjungan,
                         };

            // Filter berdasarkan search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.TipePasien.Contains(search) || u.NoRekamMedis.Contains(search)
                );
            }

            // Filter berdasarkan daterange jika keduanya memiliki nilai
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();

                query = query.Where(u =>
                    u.CreateDateTime >= startUtc &&
                    u.CreateDateTime <= endUtc);
            }

            // Filter berdasarkan periode (Hari Ini, Minggu Ini, dll)
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
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "TipePasien" => query.OrderByDescending(u => u.TipePasien),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    "NoRekamMedis" => query.OrderByDescending(u => u.NoRekamMedis),
                    "TipePasien" => query.OrderByDescending(u => u.TipePasien),
                    _ => query.OrderBy(u => u.CreateDateTime)
                };

            // Pagination
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
