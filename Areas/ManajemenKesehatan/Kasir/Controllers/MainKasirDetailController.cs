using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Interfaces;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Services;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class MainKasirDetailController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<MainKasirDetailController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INoKwitansiService _noKwitansiService;
        private readonly IGenerateUrutanAngsuran _generateUrutanAngsuran;
        private IBillingService _billingService;
        public MainKasirDetailController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<MainKasirDetailController> logger,
            IWebHostEnvironment webHostEnvironment,
            IGenerateUrutanAngsuran generateUrutanAngsuran,
            INoKwitansiService noKwitansiService,
            IBillingService billingService
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _generateUrutanAngsuran = generateUrutanAngsuran;
            _noKwitansiService = noKwitansiService;
            _billingService = billingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int perPage = 10)
        {
            // Validasi agar page dan perPage minimal bernilai 1
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // Query data
            var query = (from a in _applicationDbContext.MainKasirDetails.AsNoTracking()
                         where a.IsDelete != true
                         join u0 in _applicationDbContext.UserActives.AsNoTracking()
                             on a.CreateBy equals u0.UserActiveId into uj
                         from u in uj.DefaultIfEmpty()
                         select new
                         {
                             a.CreateDateTime,
                             a.CreateBy,
                             CreateByName = u != null ? u.FullName : null, // ✅ kalau null tetap tampil
                             a.MainKasirDetailId,
                             a.MainKasirId,
                             a.MetodePembayaranId,
                             a.ReferenceId,
                             a.NamaMetode,
                             a.NominalPembayaran,
                             a.Keterangan,
                             a.TglPembayaran,
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
            var listdata = _applicationDbContext.MainKasirDetails.Find(id);
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

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] MainKasirDetailViewModel vm, CancellationToken ct)
        //{
        //    if (vm == null || !ModelState.IsValid)
        //        return BadRequest(new { message = "Data tidak valid." });

        //    if (vm.MainKasirId == Guid.Empty)
        //        return BadRequest(new { message = "MainKasirId wajib diisi." });

        //    if (!await _applicationDbContext.Database.CanConnectAsync())
        //        return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

        //    // Ambil User ID dari JWT Claims
        //    var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(emailLogin))
        //        return Unauthorized(new { message = "User tidak terautentikasi!" });

        //    var userActiveId = await _applicationDbContext.UserActives
        //        .Where(u => u.Email == emailLogin)
        //        .Select(u => (Guid?)u.UserActiveId)
        //        .FirstOrDefaultAsync();

        //    if (!userActiveId.HasValue)
        //        return Unauthorized(new { message = "User aktif tidak ditemukan!" });

        //    await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // 1) Ambil header MainKasir
        //        var header = await _applicationDbContext.MainKasirs
        //            .FirstOrDefaultAsync(h => h.KasirId == vm.MainKasirId && !h.IsDelete);

        //        if (header == null)
        //            return NotFound(new { message = "MainKasir (header) tidak ditemukan." });

        //        if (!header.KunjunganId.HasValue || header.KunjunganId.Value == Guid.Empty)
        //            return StatusCode(500, new { message = "Header tidak memiliki KunjunganId yang valid." });

        //        var kunjunganId = header.KunjunganId.Value;

        //        // 2) Total tagihan (ambil dari header)
        //        var totalTagihan = header.GrandTotalPembayaran ?? 0m;
        //        if (totalTagihan <= 0)
        //            return BadRequest(new { message = "GrandTotalPembayaran pada header belum valid (<= 0)." });

        //        // 3) Total sudah dibayar sebelumnya (histori detail)
        //        var totalPaidBefore = await _applicationDbContext.MainKasirDetails
        //            .AsNoTracking()
        //            .Where(d => d.MainKasirId == header.KasirId)
        //            .SumAsync(d => (decimal?)(d.NominalPembayaran ?? 0m)) ?? 0m;

        //        var sisaBefore = totalTagihan - totalPaidBefore;

        //        // jika bayar lebih dri sisa maka di tolak
        //        if (vm.NominalPembayaran > sisaBefore)
        //        {
        //            return Conflict(new { message = "Biaya pembayaran melebihi tagihan" });
        //        }

        //        // Jika sudah lunas, tolak pembayaran tambahan
        //        if (sisaBefore <= 0)
        //            return Conflict(new { message = "Tagihan sudah lunas. Tidak dapat menambah pembayaran lagi." });

        //        // 4) Bayar sekarang (endpoint ini 1 metode bayar / 1 baris detail)
        //        var bayarNow = vm.NominalPembayaran ?? 0m;
        //        if (bayarNow <= 0)
        //            return BadRequest(new { message = "NominalPembayaran wajib > 0." });

        //        var totalPaidAfter = totalPaidBefore + bayarNow;
        //        var rawSisaAfter = totalTagihan - totalPaidAfter;

        //        var kembalian = rawSisaAfter < 0 ? Math.Abs(rawSisaAfter) : 0m;
        //        var sisaAfter = rawSisaAfter < 0 ? 0m : rawSisaAfter;

        //        // 5) Generate angsuran ke otomatis (lunas sekali bayar => 0, angsur => max+1)
        //        var angsuranKe = await _generateUrutanAngsuran.GenerateAsync(
        //            kunjunganId,
        //            sisaAfter,
        //            HttpContext.RequestAborted
        //        );

        //        // 6) Tentukan status header
        //        var finalStatus = (sisaAfter <= 0) ? "Lunas" : "Cicil";

        //        // 7) Insert detail
        //        var tglPembayaran = DateTimeOffset.UtcNow;

        //        var data = new MainKasirDetail
        //        {
        //            MainKasirDetailId = Guid.NewGuid(),
        //            MainKasirId = header.KasirId,

        //            // ✅ ambil dari header agar konsisten
        //            KunjunganId = header.KunjunganId,
        //            PasienId = header.PasienId,

        //            // ✅ konsisten untuk histori cicilan
        //            TotalPembayaran = totalTagihan,
        //            NominalPembayaran = bayarNow,
        //            SisaPembayaran = sisaAfter,
        //            AngsuranKe = angsuranKe,

        //            MetodePembayaranId = vm.MetodePembayaranId,
        //            ReferenceId = vm.ReferenceId,
        //            NamaMetode = vm.NamaMetode,
        //            Keterangan = vm.Keterangan,

        //            // kalau viewmodel kamu punya InvoiceBilling, tinggal isi:
        //            NoKwitansi = await _noKwitansiService.GenerateNoKwitansiAsync(tglPembayaran, ct),

        //            TglPembayaran = tglPembayaran.UtcDateTime,

        //            CreateBy = userActiveId.Value,
        //            CreateDateTime = DateTimeOffset.UtcNow,
        //        };

        //        _applicationDbContext.MainKasirDetails.Add(data);

        //        // 8) Update header (status + tanggal bayar terakhir)
        //        header.StatusPembayaran = finalStatus;
        //        header.TglPembayaran = tglPembayaran;

        //        // OPTIONAL: kalau kamu ingin NoKwitansi terbaru per pembayaran
        //        // (ingat: ini menimpa NoKwitansi sebelumnya karena kolomnya hanya ada di header)
        //        // header.NoKwitansi = await _noKwitansiService.GenerateNoKwitansiAsync(tglPembayaran, HttpContext.RequestAborted);

        //        header.UpdateBy = userActiveId.Value;
        //        header.UpdateDateTime = DateTimeOffset.UtcNow;

        //        // 9) Save
        //        var saved = await _applicationDbContext.SaveChangesAsync();
        //        if (saved <= 0)
        //        {
        //            await trx.RollbackAsync();
        //            return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
        //        }

        //        // 10) Kalau berubah jadi lunas, update billing
        //        int affectedBilling = 0;
        //        var becameLunas = (sisaBefore > 0 && sisaAfter <= 0);
        //        if (becameLunas)
        //        {
        //            affectedBilling = await _billingService.MarkBillingAsPaidAsync(kunjunganId);
        //        }

        //        await trx.CommitAsync();

        //        return Created("", new
        //        {
        //            message = "Tambah pembayaran berhasil || 201 Created",
        //            mainKasirId = header.KasirId,
        //            kunjunganId = kunjunganId,
        //            angsuranKe = angsuranKe,
        //            totalTagihan = totalTagihan,
        //            totalPaidBefore = totalPaidBefore,
        //            bayarNow = bayarNow,
        //            sisaPembayaran = sisaAfter,
        //            kembalian = kembalian,
        //            statusPembayaran = finalStatus,
        //            billingUpdated = affectedBilling
        //        });
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        await trx.RollbackAsync();
        //        return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
        //    }
        //    catch (Exception ex)
        //    {
        //        await trx.RollbackAsync();
        //        return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> CreateSplit(
        [FromBody] List<MainKasirDetailViewModel> vms,
        CancellationToken ct)
        {
            if (vms == null || vms.Count == 0)
                return BadRequest(new { message = "Data tidak valid / kosong." });

            if (!ModelState.IsValid)
                return BadRequest(new { message = "ModelState tidak valid." });

            // semua baris harus untuk header yang sama
            var mainKasirId = vms[0].MainKasirId;
            if (mainKasirId == Guid.Empty)
                return BadRequest(new { message = "MainKasirId wajib diisi." });

            if (vms.Any(x => x.MainKasirId != mainKasirId))
                return BadRequest(new { message = "Semua item harus punya MainKasirId yang sama." });

            if (!await _applicationDbContext.Database.CanConnectAsync(ct))
                return StatusCode(500, new { message = "Tidak dapat terhubung ke database." });

            // Ambil User ID dari JWT Claims
            var emailLogin = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var userActiveId = await _applicationDbContext.UserActives
                .Where(u => u.Email == emailLogin)
                .Select(u => (Guid?)u.UserActiveId)
                .FirstOrDefaultAsync(ct);

            if (!userActiveId.HasValue)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            // ✅ disarankan serializable supaya aman dari concurrent split-payment
            await using var trx = await _applicationDbContext.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

            try
            {
                // 1) Ambil header MainKasir (tracked)
                var header = await _applicationDbContext.MainKasirs
                    .FirstOrDefaultAsync(h => h.KasirId == mainKasirId && !h.IsDelete, ct);

                if (header == null)
                    return NotFound(new { message = "MainKasir (header) tidak ditemukan." });

                if (!header.KunjunganId.HasValue || header.KunjunganId.Value == Guid.Empty)
                    return StatusCode(500, new { message = "Header tidak memiliki KunjunganId yang valid." });

                var kunjunganId = header.KunjunganId.Value;

                // OPTIONAL: advisory lock per kunjungan (PostgreSQL) biar 100% aman
                // await _applicationDbContext.Database.ExecuteSqlRawAsync(
                //     "SELECT pg_advisory_xact_lock({0});",
                //     new object[] { StableHash32($"kasir:{kunjunganId}") },
                //     ct);

                // 2) Total tagihan (ambil dari header)
                var totalTagihan = header.GrandTotalPembayaran ?? 0m;
                if (totalTagihan <= 0)
                    return BadRequest(new { message = "GrandTotalPembayaran pada header belum valid (<= 0)." });

                // 3) Total sudah dibayar sebelumnya (histori detail)
                var totalPaidBefore = await _applicationDbContext.MainKasirDetails
                    .AsNoTracking()
                    .Where(d => d.MainKasirId == header.KasirId && d.IsDelete != true)
                    .SumAsync(d => (decimal?)(d.NominalPembayaran ?? 0m), ct) ?? 0m;

                // total sisa sebelum split (berdasarkan histori)
                var sisaStart = totalTagihan - totalPaidBefore;
                if (sisaStart <= 0)
                    return Conflict(new { message = "Tagihan sudah lunas. Tidak dapat menambah pembayaran lagi." });

                // validasi: total split tidak boleh melebihi sisa (kalau kamu mau strict)
                var totalSplit = vms.Sum(x => x.NominalPembayaran ?? 0m);
                if (totalSplit <= 0)
                    return BadRequest(new { message = "Total NominalPembayaran wajib > 0." });

                if (totalSplit > sisaStart)
                    return Conflict(new { message = "Total pembayaran melebihi sisa tagihan." });

                // sisa akhir setelah semua metode dibayarkan
                var sisaAfterFinal = sisaStart - totalSplit;

                // angsuranKe dihitung sekali (karena 1 transaksi = 1 angsuran)
                var angsuranKe = await _generateUrutanAngsuran.GenerateAsync(
                    kunjunganId,
                    sisaAfterFinal,
                    ct
                );

                // status header berdasarkan sisa akhir
                var finalStatus = (sisaAfterFinal <= 0) ? "Lunas" : "Cicil";

                var tglPembayaran = DateTimeOffset.Now;

                // running sisa per detail
                decimal cumulativePaid = 0m;

                foreach (var vm in vms)
                {
                    var bayarNow = vm.NominalPembayaran ?? 0m;
                    if (bayarNow <= 0) continue;

                    cumulativePaid += bayarNow;

                    var sisaPerDetail = sisaStart - cumulativePaid;
                    if (sisaPerDetail < 0) sisaPerDetail = 0; // safety

                    var detail = new MainKasirDetail
                    {
                        MainKasirDetailId = Guid.NewGuid(),
                        MainKasirId = header.KasirId,

                        KunjunganId = header.KunjunganId,
                        PasienId = header.PasienId,

                        TotalPembayaran = totalTagihan,
                        NominalPembayaran = bayarNow,

                        // ✅ ini yang berubah: sisa per baris sesuai urutan pembayaran
                        SisaPembayaran = sisaPerDetail,

                        // ✅ semua baris 1 transaksi -> 1 angsuran
                        AngsuranKe = angsuranKe,

                        MetodePembayaranId = vm.MetodePembayaranId,
                        ReferenceId = vm.ReferenceId,
                        NamaMetode = vm.NamaMetode,
                        Keterangan = vm.Keterangan,

                        // kwitansi unik per baris
                        NoKwitansi = await _noKwitansiService.GenerateNoKwitansiAsync(tglPembayaran, ct),

                        // kamu bilang tidak mau UTC
                        TglPembayaran = tglPembayaran.DateTime,

                        CreateBy = userActiveId.Value,
                        CreateDateTime = DateTimeOffset.Now,
                    };

                    _applicationDbContext.MainKasirDetails.Add(detail);
                };
                // 10) Update header
                header.StatusPembayaran = finalStatus;
                header.TglPembayaran = tglPembayaran;
                header.UpdateBy = userActiveId.Value;
                header.UpdateDateTime = DateTimeOffset.UtcNow;

                // 11) Save
                var saved = await _applicationDbContext.SaveChangesAsync(ct);
                if (saved <= 0)
                {
                    await trx.RollbackAsync(ct);
                    return StatusCode(500, new { message = "Data tidak berhasil disimpan ke database." });
                }

                // 12) Kalau berubah jadi lunas, update billing
                int affectedBilling = 0;
                var becameLunas = (sisaStart > 0 && sisaAfterFinal <= 0);
                if (becameLunas)
                {
                    affectedBilling = await _billingService.MarkBillingKunjunganAsPaidAsync(kunjunganId, (Guid)userActiveId, ct);
                }

                await trx.CommitAsync(ct);

                return Created("", new
                {
                    message = "Split payment berhasil || 201 Created",
                    mainKasirId = header.KasirId,
                    kunjunganId = kunjunganId,
                    angsuranKe = angsuranKe,
                    totalTagihan = totalTagihan,
                    totalPaidBefore = totalPaidBefore,
                    sisaPembayaran = sisaAfterFinal,
                    statusPembayaran = finalStatus,

                    billingUpdated = affectedBilling
                });
            }
            catch (DbUpdateException dbEx)
            {
                await trx.RollbackAsync(ct);
                return StatusCode(500, new { message = $"Gagal menyimpan data: {dbEx.InnerException?.Message ?? dbEx.Message}" });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync(ct);
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MainKasirDetailViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid." });
            }

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
                var data = await _applicationDbContext.MainKasirDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Update Data**
                data.MainKasirId = vm.MainKasirId;
                data.MetodePembayaranId = vm.MetodePembayaranId;
                data.ReferenceId = vm.ReferenceId;
                data.NamaMetode = vm.NamaMetode;
                data.NominalPembayaran = vm.NominalPembayaran;
                data.Keterangan = vm.Keterangan;

                data.UpdateBy = userActiveId;
                data.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.MainKasirDetails.Update(data);
                int result = await _applicationDbContext.SaveChangesAsync();

                if (result > 0)
                {
                    return Ok(new { message = "Update Data Berhasil || 200 OK" });
                }
                else
                {
                    return StatusCode(500, new { message = "Data tidak berhasil diperbarui." });
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
                var data = await _applicationDbContext.MainKasirDetails.FindAsync(id);
                if (data == null)
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }

                // **Soft Delete (Tandai Data sebagai Terhapus)**
                data.DeleteBy = userActiveId;
                data.DeleteDateTime = DateTimeOffset.UtcNow;

                data.IsDelete = true;

                _applicationDbContext.MainKasirDetails.Update(data);
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
        public async Task<IActionResult> Paged(
        int page = 1,
        int perPage = 10,
        Guid? petugasId = null,
        Guid? kasirId = null,
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
            var query =
                from d in _applicationDbContext.MainKasirDetails.AsNoTracking()
                where d.IsDelete != true

                join mk0 in _applicationDbContext.MainKasirs.AsNoTracking()
                    on d.MainKasirId equals mk0.KasirId into mkj
                from mk in mkj.DefaultIfEmpty()

                join pp0 in _applicationDbContext.PendaftaranPasienBarus.AsNoTracking()
                    on (Guid?)(mk != null ? mk.PasienId : null) equals pp0.PendaftaranPasienBaruId into ppj
                from pp in ppj.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kunjungans.AsNoTracking()
                    on (Guid?)(mk != null ? mk.KunjunganId : null) equals k0.KunjunganID into kj
                from k in kj.DefaultIfEmpty()

                join a0 in _applicationDbContext.Asuransis.AsNoTracking()
                    on (Guid?)(k != null ? k.AsuransiId : null) equals a0.AsuransiId into aj
                from a in aj.DefaultIfEmpty()

                join u0 in _applicationDbContext.UserActives.AsNoTracking()
                    on d.CreateBy equals u0.UserActiveId into uj
                from u in uj.DefaultIfEmpty()

                join pk in _applicationDbContext.Polikliniks.AsNoTracking()
                    on k.PoliklinikId equals pk.PoliklinikId into pkg
                from pk in pkg.DefaultIfEmpty()

                //join mp in _applicationDbContext.MetodePembayarans.AsNoTracking()
                //    on d.MetodePembayaranId equals mp.MetodePembayaranId into mpg
                //from mp in mpg.DefaultIfEmpty()

                select new
                {
                    d.CreateDateTime,
                    d.CreateBy,
                    CreateByName = u != null ? u.FullName : null,

                    d.MainKasirDetailId,
                    d.MainKasirId,
                    d.MetodePembayaranId,
                    d.ReferenceId,
                    d.NamaMetode,
                    d.NominalPembayaran,
                    d.Keterangan,
                    d.TglPembayaran,
                    d.NoKwitansi,
                    mk.GrandTotalPembayaran,

                    PasienId = (Guid?)(mk != null ? mk.PasienId : null),
                    KunjunganId = (Guid?)(mk != null ? mk.KunjunganId : null),
                    PoliklinikId = k.PoliklinikId,
                    NamaPoliklinik = pk.NamaPoliklinik,
                    AsalUnit = k.AsalKunjungan,
                    JenisKunjungan = k.JenisKunjungan,
                    NoRekamMedis = pp != null ? pp.NoRekamMedis : null,
                    NamaPasien = pp != null ? pp.NamaLengkap : null,
                    AsuransiId = (Guid?)(k != null ? k.AsuransiId : null),
                    NamaAsuransi = a != null ? a.NamaAsuransi : null,
                    IsPks = a != null ? a.IsPKS : null,
                };

            // **Filter berdasarkan search (Perbaikan agar bisa mencari 1 huruf)**
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = $"%{search.ToLower()}%"; // Format wildcard untuk PostgreSQL ILIKE
            //    query = query.Where(u =>
            //        EF.Functions.ILike(u.NamaDiskon, search)
            //    );
            //}

            // filter based on kunjungan id
            if (kunjunganId.HasValue)
            {
                query = query.Where(u=>u.KunjunganId == kunjunganId.Value);
            }
            // filter based on kasir id
            if (kasirId.HasValue)
            {
                query = query.Where(u=>u.MainKasirId == kasirId.Value);
            }
            // filter based on petugas kasir id
            if (petugasId.HasValue)
            {
                query = query.Where(u=>u.CreateBy == petugasId.Value);
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
                    //"NamaDiskon" => query.OrderByDescending(u => u.NamaDiskon),
                    _ => query.OrderByDescending(u => u.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(u => u.CreateDateTime),
                    "CreateByName" => query.OrderBy(u => u.CreateByName),
                    //"NamaDiskon" => query.OrderBy(u => u.NamaDiskon),
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
