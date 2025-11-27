using System.Linq;
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
                             a.PetugasId,
                             a.NoOrder,
                             a.StatusOperasi,
                             a.DepartementId
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
                return BadRequest(new { message = "Data tidak valid." });

            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // ================================
                // ✔ Ambil User Login
                // ================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;

                // set today
                var today = DateTime.UtcNow.Date;
                // ================================
                // ✔ Cek Duplikasi Booking
                // ================================
                bool isDuplicate = await _applicationDbContext.RuangBedahBookings
                    .AnyAsync(c => c.KunjunganId == vm.KunjunganId && c.IsDelete == false && c.CreateDateTime==today);

                if (isDuplicate)
                    return Conflict(new { message = "Kunjungan ini telah booking ruang bedah untuk hari ini" });


                // ================================
                // ✔ Generate NoOrder
                // ================================
                string prefix = (bool)vm.isBedahBersalin ? "OBS" : "BED";
                string datePart = today.ToString("yyyyMMdd");

                var lastOrderToday = await _applicationDbContext.RuangBedahBookings
                    .Where(x => x.CreateDateTime.Date == today && x.NoOrder.StartsWith(prefix))
                    .OrderByDescending(x => x.NoOrder)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (lastOrderToday != null)
                {
                    string lastNumberPart = lastOrderToday.NoOrder.Substring(prefix.Length + 8);
                    if (int.TryParse(lastNumberPart, out int lastNum))
                        nextNumber = lastNum + 1;
                }

                string noOrder = $"{prefix}{datePart}{nextNumber:D4}";


                // ================================
                // ✔ Insert Parent
                // ================================
                var parentId = Guid.NewGuid();

                var parent = new RuangBedahBooking
                {
                    BookingRuanganBedahId = parentId,
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
                    TipeOperasi = vm.TipeOperasi,
                    JamPerpanjangan = vm.JamPerpanjangan,
                    BiayaPerpanjangan = vm.BiayaPerpanjangan,
                    KamarRecoveryId = vm.KamarRecoveryId,
                    TipeAnastesiId = vm.TipeAnastesiId,
                    TipeASAId = vm.TipeASAId,
                    KelompokPasienAnastesi = vm.KelompokPasienAnastesi,
                    PetugasId = vm.PetugasId,
                    NoOrder = noOrder,
                    StatusOperasi = vm.StatusOperasi,
                    DepartementId = vm.DepartementId,

                    CreateBy = userActiveId,
                    CreateDateTime = DateTimeOffset.UtcNow,
                };

                await _applicationDbContext.RuangBedahBookings.AddAsync(parent);
                await _applicationDbContext.SaveChangesAsync();


                // ================================
                // ✔ Insert Child (Details)
                // ================================
                if (vm.Details != null && vm.Details.Any())
                {
                    List<RuangBedahBookingDetail> detailList = new();

                    foreach (var d in vm.Details)
                    {
                        var detail = new RuangBedahBookingDetail
                        {
                            DetailBookingBedahId = Guid.NewGuid(),
                            BookingRuanganBedahId = parentId,
                            JenisOperasiId = d.JenisOperasiId,
                            TindakanId = d.TindakanId,       // ← ARRAY<Guid>
                            UserActiveId = d.UserActiveId,   // ← ARRAY<Guid>
                            PersentaseTindakan = d.PersentaseTindakan,
                            DiskonDokter = d.DiskonDokter,
                            Keterangan = d.Keterangan,
                            CreateBy = userActiveId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        detailList.Add(detail);
                    }

                    await _applicationDbContext.RuangBedahBookingDetails.AddRangeAsync(detailList);
                    await _applicationDbContext.SaveChangesAsync();
                }


                // ================================
                // ✔ Commit Transaction
                // ================================
                await transaction.CommitAsync();


                // ================================
                // ✔ Response Output
                // ================================
                return Ok(new
                {
                    message = "Booking ruang bedah + detail berhasil ditambahkan",
                    BookingRuanganBedahId = parentId,
                    NoOrder = noOrder,
                    JumlahDetail = vm.Details?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RuangBedahBookingViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

            try
            {
                // ================================
                // ✔ Ambil User Login
                // ================================
                var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(emailLogin))
                    return Unauthorized(new { message = "User tidak terautentikasi!" });

                var getUserActive = await _applicationDbContext.UserActives
                    .FirstOrDefaultAsync(u => u.Email == emailLogin);

                if (getUserActive == null)
                    return Unauthorized(new { message = "User aktif tidak ditemukan!" });

                var userActiveId = getUserActive.UserActiveId;


                // ================================
                // ✔ Ambil Parent
                // ================================
                var parent = await _applicationDbContext.RuangBedahBookings
                    .FirstOrDefaultAsync(x => x.BookingRuanganBedahId == id && x.IsDelete == false);

                if (parent == null)
                    return NotFound(new { message = "Data Booking Ruang Bedah tidak ditemukan!" });


                // ================================
                // ✔ Update Parent
                // ================================
                parent.KunjunganId = vm.KunjunganId;
                parent.PasienId = vm.PasienId;
                parent.TglOperasi = vm.TglOperasi;
                parent.WaktuOperasi = vm.WaktuOperasi;
                parent.RuangTindakan = vm.RuangTindakan;
                parent.DiagnosaDokter1 = vm.DiagnosaDokter1;
                parent.DiagnosaDokter2 = vm.DiagnosaDokter2;
                parent.DiagnosaDokter3 = vm.DiagnosaDokter3;
                parent.DiagnosaDokter4 = vm.DiagnosaDokter4;
                parent.DiagnosaDokter5 = vm.DiagnosaDokter5;
                parent.BeratBadan = vm.BeratBadan;
                parent.DokterOperator1 = vm.DokterOperator1;
                parent.DokterOperator2 = vm.DokterOperator2;
                parent.DokterOperator3 = vm.DokterOperator3;
                parent.DokterOperator4 = vm.DokterOperator4;
                parent.DokterOperator5 = vm.DokterOperator5;
                parent.RencanaTindakanOperasi = vm.RencanaTindakanOperasi;
                parent.JenisAnastesi = vm.JenisAnastesi;
                parent.TypeOK = vm.TypeOK;
                parent.PenandaanLokasiOperasi = vm.PenandaanLokasiOperasi;
                parent.isBedahBersalin = vm.isBedahBersalin;
                parent.Keterangan = vm.Keterangan;
                parent.TipeTindakan = vm.TipeTindakan;
                parent.TipeOperasi = vm.TipeOperasi;
                parent.JamPerpanjangan = vm.JamPerpanjangan;
                parent.BiayaPerpanjangan = vm.BiayaPerpanjangan;
                parent.KamarRecoveryId = vm.KamarRecoveryId;
                parent.TipeAnastesiId = vm.TipeAnastesiId;
                parent.TipeASAId = vm.TipeASAId;
                parent.KelompokPasienAnastesi = vm.KelompokPasienAnastesi;
                parent.PetugasId = vm.PetugasId;
                parent.StatusOperasi = vm.StatusOperasi;
                parent.DepartementId = vm.DepartementId;

                parent.UpdateBy = userActiveId;
                parent.UpdateDateTime = DateTimeOffset.UtcNow;

                await _applicationDbContext.SaveChangesAsync();


                // ================================
                // ✔ Update Detail Tanpa Tambah/Hapus
                // ================================
                if (vm.Details != null && vm.Details.Any())
                {
                    foreach (var d in vm.Details)
                    {
                        var detail = await _applicationDbContext.RuangBedahBookingDetails
                            .FirstOrDefaultAsync(x => x.DetailBookingBedahId == d.DetailBookingBedahId);

                        if (detail == null)
                            return BadRequest(new
                            {
                                message = $"Detail dengan ID {d.DetailBookingBedahId} tidak ditemukan!"
                            });

                        // Update detail
                        detail.JenisOperasiId = d.JenisOperasiId;
                        detail.TindakanId = d.TindakanId;
                        detail.UserActiveId = d.UserActiveId;
                        detail.PersentaseTindakan = d.PersentaseTindakan;
                        detail.DiskonDokter = d.DiskonDokter;
                        detail.Keterangan = d.Keterangan;

                        detail.UpdateBy = userActiveId;
                        detail.UpdateDateTime = DateTimeOffset.UtcNow;
                    }

                    await _applicationDbContext.SaveChangesAsync();
                }


                // ================================
                // ✔ Commit Transaction
                // ================================
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Update Booking Ruang Bedah + Detail berhasil",
                    BookingRuanganBedahId = id,
                    JumlahDetailDiupdate = vm.Details?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
             DateTime? startDate = null,
             DateTime? endDate = null,
             PeriodeFilter? periode = null)
        {
            // ============================
            // 1️⃣ QUERY PARENT
            // ============================
            var parentQuery =
                from a in _applicationDbContext.RuangBedahBookings
                join u in _applicationDbContext.UserActives on a.CreateBy equals u.UserActiveId into uGroup
                from u in uGroup.DefaultIfEmpty()
                where (a.IsDelete == false || a.IsDelete == null)
                select new
                {
                    a.BookingRuanganBedahId,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u.FullName,
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
                    a.PetugasId,
                    a.NoOrder,
                    a.StatusOperasi,
                    a.DepartementId,
                };

            // ============================
            // 2️⃣ FILTER
            // ============================

            if (kunjunganId.HasValue)
                parentQuery = parentQuery.Where(x => x.KunjunganId == kunjunganId.Value);

            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                parentQuery = parentQuery.Where(x =>
                    x.CreateDateTime >= startUtc &&
                    x.CreateDateTime <= endUtc);
            }

            if (periode.HasValue)
            {
                DateTime today = DateTime.UtcNow.Date;

                switch (periode)
                {
                    case PeriodeFilter.Today:
                        parentQuery = parentQuery.Where(u => u.CreateDateTime.Date == today);
                        break;
                    case PeriodeFilter.ThisWeek:
                        parentQuery = parentQuery.Where(u =>
                            u.CreateDateTime.Date >= today.AddDays(-(int)today.DayOfWeek) &&
                            u.CreateDateTime.Date <= today);
                        break;
                }
            }

            // ============================
            // 3️⃣ SORT
            // ============================
            parentQuery = sortDirection?.ToLower() == "desc"
                ? orderBy switch
                {
                    "CreateByName" => parentQuery.OrderByDescending(x => x.CreateByName),
                    _ => parentQuery.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateByName" => parentQuery.OrderBy(x => x.CreateByName),
                    _ => parentQuery.OrderBy(x => x.CreateDateTime)
                };

            // ============================
            // 4️⃣ PAGING PARENT
            // ============================
            int totalRows = parentQuery.Count();
            int totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            var pagedParents = parentQuery
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToList();

            if (!pagedParents.Any())
                return Ok(new
                {
                    status = "success",
                    data = new { Rows = new List<object>(), TotalRows = 0 }
                });

            var parentIds = pagedParents.Select(x => x.BookingRuanganBedahId).ToList();

            // ============================
            // 5️⃣ LOAD DETAIL SEKALI SAJA (ANTI-N+1)
            // ============================
            var details =
                (from d in _applicationDbContext.RuangBedahBookingDetails
                 where parentIds.Contains((Guid)d.BookingRuanganBedahId)
                 select new
                 {
                     d.DetailBookingBedahId,
                     d.BookingRuanganBedahId,
                     d.JenisOperasiId,
                     d.TindakanId,
                     d.UserActiveId,
                     d.PersentaseTindakan,
                     d.DiskonDokter,
                     d.Keterangan
                 }).ToList();

            // ============================
            // 6️⃣ MERGE PARENT + DETAIL
            // ============================
            var merged = pagedParents.Select(p => new
            {
                Parent = p,
                Details = details.Where(d => d.BookingRuanganBedahId == p.BookingRuanganBedahId).ToList()
            });

            // ============================
            // 7️⃣ RETURN
            // ============================
            return Ok(new
            {
                status = "success",
                message = "Data retrieved successfully",
                data = new
                {
                    Rows = merged,
                    TotalRows = totalRows,
                    CurrentPage = page,
                    PerPage = perPage,
                    TotalPages = totalPages
                }
            });
        }



    }
}
