using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Farmasi.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.OperasiOK.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class RuangBedahBookingController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<RuangBedahBookingController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RuangBedahBookingController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RuangBedahBookingController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.RuangBedahBookings
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.BookingRuanganBedahId,
                             a.KunjunganId,
                             a.PasienId,
                             a.TglOperasi,
                             a.WaktuOperasi,
                             a.RuangTindakan,
                             a.DiagnosaDokter1,
                             a.DiagnosaDokter2,
                             a.DiagnosaDokter3,
                             a.DiagnosaDokter4,
                             a.DiagnosaDokter5,
                             a.BeratBadan,
                             a.DokterOperator1,
                             a.DokterOperator2,
                             a.DokterOperator3,
                             a.DokterOperator4,
                             a.DokterOperator5,
                             a.RencanaTindakanOperasi,
                             a.JenisAnastesi,
                             a.TypeOK,
                             a.PenandaanLokasiOperasi,
                             a.isSuratIzinOperasi,
                             a.isBedahBersalin,
                             a.Keterangan,
                             a.IsTerverifikasi,
                             a.TglSelesai,
                             a.TipeTindakan,
                             a.TipeOperasi,
                             a.JamPerpanjangan,
                             a.BiayaPerpanjangan,
                             a.KamarRecoveryId,
                             a.TipeAnastesiId,
                             a.TipeASAId,
                             a.KelompokPasienAnastesi,
                         }).OrderByDescending(a => a.CreateDateTime);

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
        public async Task<IActionResult> GetById(Guid id)
        {
            var listdata = _applicationDbContext.RuangBedahBookings.Find(id);
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
        public async Task<IActionResult> Create([FromBody] RuangBedahBookingViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                //// **Cek Duplikasi**
                bool isDuplicate = await _applicationDbContext.RuangBedahBookings
                                    .AnyAsync(c => c.KunjunganId == vm.KunjunganId && c.IsDelete==false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Kunjungan ini telah booking ruang bedah" });
                }

                // ==========================================================
                // ✅ Generate Nomor Order
                // ==========================================================
                // Prefix: 3 huruf, tergantung dari isBedahBersalin
                string prefix = (bool)vm.isBedahBersalin ? "OBS" : "BED"; // OBS = Obstetri, BED = Bedah umum
                var today = DateTime.UtcNow.Date;
                string datePart = today.ToString("yyyyMMdd");

                // Cari order terakhir hari ini dengan prefix sesuai
                var lastOrderToday = await _applicationDbContext.RuangBedahBookings
                    .Where(x => x.CreateDateTime.Date == today && x.NoOrder.StartsWith(prefix))
                    .OrderByDescending(x => x.NoOrder)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (lastOrderToday != null)
                {
                    // Format: BED202511070001 → ambil angka terakhir
                    string lastNumberPart = lastOrderToday.NoOrder.Substring(prefix.Length + 8); // lewati prefix + tanggal (8)
                    if (int.TryParse(lastNumberPart, out int lastNum))
                        nextNumber = lastNum + 1;
                }

                string noOrder = $"{prefix}{datePart}{nextNumber:D4}";

                // **Buat Data Baru**
                var data = new RuangBedahBooking
                {
                    BookingRuanganBedahId = Guid.NewGuid(),
                    KunjunganId = vm.KunjunganId,
                    PasienId = vm.PasienId,
                    TglOperasi = vm.TglOperasi,
                    WaktuOperasi = vm.WaktuOperasi,
                    RuangTindakan = vm.RuangTindakan,
                    DiagnosaDokter1 = vm.DiagnosaDokter1,
                    DiagnosaDokter2 = vm.DiagnosaDokter2,
                    DiagnosaDokter3 = vm.DiagnosaDokter3,
                    DiagnosaDokter4 = vm.DiagnosaDokter4,
                    DiagnosaDokter5 = vm.DiagnosaDokter5,
                    BeratBadan = vm.BeratBadan,
                    DokterOperator1 = vm.DokterOperator1,
                    DokterOperator2 = vm.DokterOperator2,
                    DokterOperator3 = vm.DokterOperator3,
                    DokterOperator4 = vm.DokterOperator4,
                    DokterOperator5 = vm.DokterOperator5,
                    RencanaTindakanOperasi = vm.RencanaTindakanOperasi,
                    JenisAnastesi = vm.JenisAnastesi,
                    TypeOK = vm.TypeOK,
                    PenandaanLokasiOperasi = vm.PenandaanLokasiOperasi,
                    isBedahBersalin = vm.isBedahBersalin,
                    isSuratIzinOperasi = false,
                    Keterangan = vm.Keterangan,
                    TipeTindakan = vm.TipeTindakan,
                    IsTerverifikasi = false,
                    TipeOperasi = vm.TipeOperasi,
                    JamPerpanjangan = vm.JamPerpanjangan,
                    BiayaPerpanjangan = vm.BiayaPerpanjangan,
                    KamarRecoveryId = vm.KamarRecoveryId,
                    TipeAnastesiId = vm.TipeAnastesiId,
                    TipeASAId = vm.TipeASAId,
                    KelompokPasienAnastesi = vm.KelompokPasienAnastesi,
                    PetugasId = vm.PetugasId,
                    NoOrder = noOrder,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                // **Simpan ke Database**
                _applicationDbContext.RuangBedahBookings.Add(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Created("", new { message = "Tambah Data Berhasil || 201 Created" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RuangBedahBookingViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

            try
            {
                // **Cek koneksi ke database**
                if (!_applicationDbContext.Database.CanConnect())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cek apakah data ada**
                var existingData = await _applicationDbContext.RuangBedahBookings
                                        .FirstOrDefaultAsync(c => c.BookingRuanganBedahId == id && (c.IsDelete == false || c.IsDelete == null));

                if (existingData == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                bool isDuplicate = await _applicationDbContext.RuangBedahBookings
                    .AnyAsync(c => c.KunjunganId == vm.KunjunganId &&c.BookingRuanganBedahId!=id && c.IsDelete == false);

                if (isDuplicate)
                {
                    return Conflict(new { message = "Kunjungan ini telah booking ruang bedah" });
                }

                // **Update field yang diubah**
                existingData.KunjunganId = vm.KunjunganId;
                existingData.PasienId = vm.PasienId;
                existingData.TglOperasi = vm.TglOperasi;
                existingData.WaktuOperasi = vm.WaktuOperasi;
                existingData.RuangTindakan = vm.RuangTindakan;
                existingData.DiagnosaDokter1 = vm.DiagnosaDokter1;
                existingData.DiagnosaDokter2 = vm.DiagnosaDokter2;
                existingData.DiagnosaDokter3 = vm.DiagnosaDokter3;
                existingData.DiagnosaDokter4 = vm.DiagnosaDokter4;
                existingData.DiagnosaDokter5 = vm.DiagnosaDokter5;
                existingData.BeratBadan = vm.BeratBadan;
                existingData.DokterOperator1 = vm.DokterOperator1;
                existingData.DokterOperator2 = vm.DokterOperator2;
                existingData.DokterOperator3 = vm.DokterOperator3;
                existingData.DokterOperator4 = vm.DokterOperator4;
                existingData.DokterOperator5 = vm.DokterOperator5;
                existingData.RencanaTindakanOperasi = vm.RencanaTindakanOperasi;
                existingData.JenisAnastesi = vm.JenisAnastesi;
                existingData.TypeOK = vm.TypeOK;
                existingData.PenandaanLokasiOperasi = vm.PenandaanLokasiOperasi;
                existingData.isBedahBersalin = vm.isBedahBersalin;
                existingData.TipeTindakan = vm.TipeTindakan;
                existingData.TipeOperasi = vm.TipeOperasi;
                existingData.JamPerpanjangan = vm.JamPerpanjangan;
                existingData.BiayaPerpanjangan = vm.BiayaPerpanjangan;
                existingData.KamarRecoveryId = vm.KamarRecoveryId;
                existingData.TipeAnastesiId = vm.TipeAnastesiId;
                existingData.TipeASAId = vm.TipeASAId;
                existingData.KelompokPasienAnastesi = vm.KelompokPasienAnastesi;
                existingData.Keterangan = vm.Keterangan;

                existingData.UpdateBy = userActiveId;
                existingData.UpdateDateTime = DateTimeOffset.UtcNow;

                // **Simpan ke Database**
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Tidak ada perubahan yang disimpan ke database." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menyimpan perubahan: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}/is-IzinOperasi")]
        public async Task<IActionResult> UpdateIzinOperasi(Guid id, [FromBody] bool request)
        {
            var data = await _applicationDbContext.RuangBedahBookings.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.isSuratIzinOperasi = request;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi signalR
            //await _hubContext.Clients.All.SendAsync("isCancelledChanged", new
            //{
            //    Action = "updateIsCancelled",
            //    ResepId = id,
            //    IsCancelled = request.IsCancelled
            //});

            return Ok(new { message = "Status izin operasi berhasil diperbarui." });
        }

        [HttpPut("{id}/Verifikasi-Operasi")]
        public async Task<IActionResult> UpdateVerifikasiOP(Guid id, [FromBody] bool request)
        {
            var data = await _applicationDbContext.RuangBedahBookings.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.IsTerverifikasi = request;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi signalR
            //await _hubContext.Clients.All.SendAsync("isCancelledChanged", new
            //{
            //    Action = "updateIsCancelled",
            //    ResepId = id,
            //    IsCancelled = request.IsCancelled
            //});

            return Ok(new { message = "Status verifikasi operasi berhasil diperbarui." });
        }

        [HttpPut("{id}/Tanggal-Selesai-Operasi")]
        public async Task<IActionResult> UpdateTglSelesaiOP(Guid id, [FromBody] DateTime request)
        {
            var data = await _applicationDbContext.RuangBedahBookings.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Resep tidak ditemukan." });

            var EmailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(EmailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = _applicationDbContext.UserActives.FirstOrDefault(u => u.Email == EmailLogin);
            var userId = user?.UserActiveId ?? Guid.Empty;

            data.TglSelesai = request;
            data.UpdateDateTime = DateTimeOffset.UtcNow;
            data.UpdateBy = userId;
            await _applicationDbContext.SaveChangesAsync();

            // Notifikasi signalR
            //await _hubContext.Clients.All.SendAsync("isCancelledChanged", new
            //{
            //    Action = "updateIsCancelled",
            //    ResepId = id,
            //    IsCancelled = request.IsCancelled
            //});

            return Ok(new { message = "Tanggal selesai operasi berhasil diperbarui." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // **Cek koneksi ke database**
                if (!await _applicationDbContext.Database.CanConnectAsync())
                {
                    return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });
                }

                // **Ambil User ID dari JWT Claims**
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(emailLogin))
                {
                    return Unauthorized(new { message = "User tidak terautentikasi!" });
                }

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);
                if (getUserActive == null)
                {
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });
                }
                var userActiveId = getUserActive.UserActiveId;

                // **Cari Data**
                var data = await _applicationDbContext.RuangBedahBookings.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.RuangBedahBookings.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Data berhasil dihapus (soft delete) || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
                }
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new { message = $"Gagal menghapus data: {dbEx.InnerException?.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpGet("paged")]
        public IActionResult Paged(
         int page = 1,
         int perPage = 10,
         Guid? kunjunganId = null,
         string? orderBy = "CreateDateTime",
         string? sortDirection = "desc",
         [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? startDate = null,
         [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
                        DateTime? endDate = null,
         [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null)
        {

            // Query data
            var query = (from a in _applicationDbContext.RuangBedahBookings
                         join u in _applicationDbContext.UserActives.DefaultIfEmpty()
                         on a.CreateBy equals u.UserActiveId
                         where a.IsDelete == false || a.IsDelete == null
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u.FullName,
                             a.BookingRuanganBedahId,
                             a.KunjunganId,
                             a.PasienId,
                             a.TglOperasi,
                             a.WaktuOperasi,
                             a.RuangTindakan,
                             a.DiagnosaDokter1,
                             a.DiagnosaDokter2,
                             a.DiagnosaDokter3,
                             a.DiagnosaDokter4,
                             a.DiagnosaDokter5,
                             a.BeratBadan,
                             a.DokterOperator1,
                             a.DokterOperator2,
                             a.DokterOperator3,
                             a.DokterOperator4,
                             a.DokterOperator5,
                             a.RencanaTindakanOperasi,
                             a.JenisAnastesi,
                             a.TypeOK,
                             a.PenandaanLokasiOperasi,
                             a.isSuratIzinOperasi,
                             a.isBedahBersalin,
                             a.Keterangan,
                             a.IsTerverifikasi,
                             a.TglSelesai,
                             a.TipeTindakan,
                             a.TipeOperasi,
                             a.JamPerpanjangan,
                             a.BiayaPerpanjangan,
                             a.KamarRecoveryId,
                             a.TipeAnastesiId,
                             a.TipeASAId,
                             a.KelompokPasienAnastesi,
                         });

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter bedasarkan kunjungan id
            if (kunjunganId.HasValue )
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
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
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today
                        );
                        break;
                    case PeriodeFilter.LastWeek:
                        query = query.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-7 - (int)today.DayOfWeek) &&
                            u.CreateDateTime.Date < today.AddDays(-(int)today.DayOfWeek)
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
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
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
