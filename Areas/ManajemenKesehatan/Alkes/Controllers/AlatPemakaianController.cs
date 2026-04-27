using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Hubs;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.HubSignalR;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class AlatPemakaianController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AlatPemakaianController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<AlatPemakaianHub> _hubContext;
        private readonly IGenerateInvoiceBillingService _generateInvoiceBillingService;
        private readonly IAsuransiCoverageService _asuransiCoverageService;
        public AlatPemakaianController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AlatPemakaianController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<AlatPemakaianHub> hubContext,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            IAsuransiCoverageService asuransiCoverageService
            )
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
            _generateInvoiceBillingService = generateInvoiceBillingService;
            _asuransiCoverageService = asuransiCoverageService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // =========================
            // 1) Ambil header
            // =========================
            var header = await _applicationDbContext.AlatPemakaians
                .AsNoTracking()
                .Where(x => x.PemakaianAlatId == id)
                .Select(x => new
                {
                    x.PemakaianAlatId,
                    x.KunjunganId,
                    x.PasienId,
                    x.TanggalPemakaian,
                    x.Keterangan,
                    x.CreateDateTime,
                    x.CreateBy
                })
                .FirstOrDefaultAsync();

            if (header == null)
                return NotFound(new { message = "Data pemakaian alat tidak ditemukan." });

            // =========================
            // 2) Ambil detail alat
            // =========================
            var details = await _applicationDbContext.AlatPemakaianDetails
                .AsNoTracking()
                .Where(x => x.PemakaianAlatId == id)
                .Select(x => new
                {
                    x.DetailPemakaianAlatId,
                    x.PeralatanId,
                    x.KelasId,
                    x.QtyPemakaian,
                    x.HargaPeralatan,
                    x.TotalPemakaianAlat,
                    x.Keterangan
                })
                .ToListAsync();

            // =========================
            // 3) Lookup Nama Peralatan & Nama Kelas
            // =========================

            var alatIds = details.Where(d => d.PeralatanId != null).Select(d => d.PeralatanId!.Value).Distinct().ToList();
            var kelasIds = details.Where(d => d.KelasId != null).Select(d => d.KelasId!.Value).Distinct().ToList();

            var namaAlatDict = await _applicationDbContext.Peralatans
                .Where(x => alatIds.Contains(x.PeralatanId))
                .Select(x => new { x.PeralatanId, x.NamaPeralatan })
                .ToDictionaryAsync(x => x.PeralatanId, x => x.NamaPeralatan);

            var kelasDict = await _applicationDbContext.Kelass
                .Where(x => kelasIds.Contains(x.KelasId))
                .Select(x => new { x.KelasId, x.NamaKelas })
                .ToDictionaryAsync(x => x.KelasId, x => x.NamaKelas);

            // =========================
            // 4) Final result (AMAN dari null)
            // =========================
            var result = new
            {
                Header = header,
                Details = details.Select(d => new
                {
                    d.DetailPemakaianAlatId,
                    d.PeralatanId,

                    NamaPeralatan =
                        d.PeralatanId != null &&
                        namaAlatDict.TryGetValue(d.PeralatanId.Value, out var alat)
                            ? alat
                            : null,

                    d.KelasId,
                    NamaKelas =
                        d.KelasId != null &&
                        kelasDict.TryGetValue(d.KelasId.Value, out var kelas)
                            ? kelas
                            : null,

                    d.QtyPemakaian,
                    d.HargaPeralatan,
                    d.TotalPemakaianAlat,
                    d.Keterangan
                })
            };

            return Ok(result);
        }


        [HttpGet("by-kunjungan/{kunjunganId}")]
        public async Task<IActionResult> GetByKunjunganId(Guid kunjunganId)
        {
            // =========================
            // 1. Ambil header pemakaian alat
            // =========================
            var headers = await _applicationDbContext.AlatPemakaians
                .AsNoTracking()
                .Where(x => x.KunjunganId == kunjunganId && !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new
                {
                    x.PemakaianAlatId,
                    x.KunjunganId,
                    x.PasienId,
                    x.TanggalPemakaian,
                    x.Keterangan,
                    x.CreateDateTime
                })
                .ToListAsync();

            if (headers.Count == 0)
                return NotFound(new { message = "Tidak ada data pemakaian alat untuk kunjungan ini." });

            var pemakaianIds = headers.Select(h => h.PemakaianAlatId).ToList();

            // =========================
            // 2. Ambil semua detail alat
            // =========================
            var details = await _applicationDbContext.AlatPemakaianDetails
                .AsNoTracking()
                .Where(x => pemakaianIds.Contains((Guid)x.PemakaianAlatId) && !x.IsDelete)
                .Select(x => new
                {
                    x.PemakaianAlatId,
                    x.DetailPemakaianAlatId,
                    x.PeralatanId,
                    x.KelasId,
                    x.QtyPemakaian,
                    x.HargaPeralatan,
                    x.TotalPemakaianAlat,
                    x.Keterangan
                })
                .ToListAsync();

            // =========================
            // 3. Load lookup master alat & kelas
            // =========================
            var alatIds = details.Where(d => d.PeralatanId != null).Select(d => d.PeralatanId!.Value).Distinct().ToList();
            var kelasIds = details.Where(d => d.KelasId != null).Select(d => d.KelasId!.Value).Distinct().ToList();

            var namaAlatDict = await _applicationDbContext.Peralatans
                .Where(x => alatIds.Contains(x.PeralatanId))
                .Select(x => new { x.PeralatanId, x.NamaPeralatan })
                .ToDictionaryAsync(x => x.PeralatanId, x => x.NamaPeralatan);

            var kelasDict = await _applicationDbContext.Kelass
                .Where(x => kelasIds.Contains(x.KelasId))
                .Select(x => new { x.KelasId, x.NamaKelas })
                .ToDictionaryAsync(x => x.KelasId, x => x.NamaKelas);

            // =========================
            // 4. Grouping hasil aman dari null
            // =========================
            var result = headers.Select(h => new
            {
                Header = h,
                Details = details
                    .Where(d => d.PemakaianAlatId == h.PemakaianAlatId)
                    .Select(d => new
                    {
                        d.DetailPemakaianAlatId,
                        d.PeralatanId,
                        NamaPeralatan =
                            d.PeralatanId != null &&
                            namaAlatDict.TryGetValue(d.PeralatanId.Value, out var nama)
                                ? nama
                                : null,

                        d.KelasId,
                        NamaKelas =
                            d.KelasId != null &&
                            kelasDict.TryGetValue(d.KelasId.Value, out var kelas)
                                ? kelas
                                : null,

                        d.QtyPemakaian,
                        d.HargaPeralatan,
                        d.TotalPemakaianAlat,
                        d.Keterangan
                    })
            });

            return Ok(new
            {
                KunjunganId = kunjunganId,
                TotalPemakaian = headers.Count,
                Data = result
            });
        }


        [HttpPost]
        public async Task<IActionResult> CreateAlatPemakaian([FromBody] AlatPemakaianViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.KunjunganId == null || vm.PasienId == null)
                return BadRequest(new { message = "KunjunganId dan PasienId wajib diisi." });

            if (vm.Details == null || vm.Details.Count == 0)
                return BadRequest(new { message = "Detail pemakaian alat wajib diisi minimal 1 item." });

            // Ambil user dari JWT
            var emailLogin = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(x => x.Email == emailLogin);
            if (user == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userId = user.UserActiveId;

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                // =========================
                // 1) billingIndex awal (khusus kunjungan & jenis billing)
                // =========================
                int billingIndex = await _applicationDbContext.Billings
                    .CountAsync(b =>
                        b.KunjunganId == vm.KunjunganId.Value &&
                        b.JenisBilling.ToLower() == "alkes");

                var billingDict = new Dictionary<Guid, Billing>(); // key: PeralatanId

                // =========================
                // 2) Insert HEADER
                // =========================
                var alatPemakaianId = Guid.NewGuid();

                var header = new AlatPemakaian
                {
                    PemakaianAlatId = alatPemakaianId,
                    KunjunganId = vm.KunjunganId.Value,
                    PasienId = vm.PasienId.Value,
                    TanggalPemakaian = vm.TanggalPemakaian ?? DateTime.UtcNow,
                    Keterangan = vm.Keterangan,
                    CreateBy = userId,
                    CreateDateTime = DateTimeOffset.UtcNow
                };
                _applicationDbContext.AlatPemakaians.Add(header);

                // =========================
                // 3) Insert DETAIL + BILLING (dengan billingIndex)
                // =========================
                var detailEntities = new List<AlatPemakaianDetail>();

                foreach (var d in vm.Details)
                {
                    if (d?.PeralatanId == null)
                        return BadRequest(new { message = "PeralatanId pada detail wajib diisi." });

                    var alatId = d.PeralatanId.Value;
                    var qtyInput = d.QtyPemakaian ?? 1;
                    if (qtyInput <= 0) qtyInput = 1;

                    // Ambil nama & harga alat (sesuaikan tabel/kolomnya)
                    //var alatDb = await _applicationDbContext.TarifKelass
                    //    .Where(x => x.PeralatanId == alatId && x.KelasId == d.KelasId )
                    //    .FirstOrDefaultAsync();

                    var namaAlat = await _applicationDbContext.Peralatans
                        .Where(x=>x.PeralatanId == alatId)
                        .Select(x=>x.NamaPeralatan)
                        .FirstOrDefaultAsync();

                    //if (alatDb == null && namaAlat==null)
                    //    return BadRequest(new { message = $"Peralatan tidak ditemukan: {alatId}" });

                    ////var harga = d.HargaPeralatan ?? alatDb.Harga;
                    //var subTotal = (alatDb.TarifRs * qtyInput);

                    // ---- DETAIL ----
                    detailEntities.Add(new AlatPemakaianDetail
                    {
                        DetailPemakaianAlatId = Guid.NewGuid(),
                        PemakaianAlatId = alatPemakaianId,
                        PeralatanId = alatId,
                        KelasId = d.KelasId,
                        QtyPemakaian = qtyInput,
                        //HargaPeralatan = alatDb.TarifRs,
                        //TotalPemakaianAlat = subTotal,
                        Keterangan = d.Keterangan,
                        CreateBy = userId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    });

                    var coverage = await _asuransiCoverageService.ResolveCoverageAsync(
                       vm.KunjunganId,
                        "Alkes",
                        alatId,
                        ct);

                    // ---- BILLING (Pola dictionary seperti contoh kamu) ----
                    if (!billingDict.TryGetValue(alatId, out var billing))
                    {
                        billingIndex++;

                        billing = new Billing
                        {

                            BillingId = Guid.NewGuid(),
                            KunjunganId = vm.KunjunganId.Value,

                            BillingDate = DateTime.UtcNow,
                            BillingKode = $"{billingIndex:D3}",
                            InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)header.KunjunganId,
                                DateTime.UtcNow),
                            IsListWhiteOff = false,
                            ItemId = alatId,
                            NamaItem = namaAlat,
                            LayananId = vm.LayananId,

                            //HargaItem = alatDb.TarifRs,
                            QtyItem = qtyInput,
                            //SubTotalItem = alatDb.TarifRs * qtyInput,

                            JenisBilling = "Alkes",
                            StatusPengambilan = true,
                            StatusBilling = false,
                            IsCovered = coverage?.IsCovered,
                            IsCoveredExcess = coverage?.IsCoveredExcess,
                            AsuransiId = coverage?.AsuransiId,
                            AsuransiExcessId = coverage?.AsuransiExcessId,

                            TanggalInvoice = DateTime.UtcNow,
                            TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                            CreateBy = userId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        billingDict[alatId] = billing;
                        _applicationDbContext.Billings.Add(billing);
                    }
                    else
                    {
                        billing.QtyItem += qtyInput;
                        billing.SubTotalItem = billing.HargaItem * billing.QtyItem;
                    }
                }

                _applicationDbContext.AlatPemakaianDetails.AddRange(detailEntities);

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                await _hubContext.Clients.All.SendAsync("Pemakaian alat ditambah", new
                {
                    action = "create",
                    alatPemakaianId
                });

                return Created("", new
                {
                    message = "Berhasil menambahkan Alat Pemakaian + Detail + Billing",
                    alatPemakaianId,
                    totalDetail = detailEntities.Count,
                    totalBilling = billingDict.Count
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlatPemakaian(Guid pemakaianAlatId, [FromBody] AlatPemakaianViewModel vm, CancellationToken ct)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.KunjunganId == null || vm.PasienId == null)
                return BadRequest(new { message = "KunjunganId dan PasienId wajib diisi." });

            if (vm.Details == null || vm.Details.Count == 0)
                return BadRequest(new { message = "Detail pemakaian alat wajib diisi minimal 1 item." });

            // Ambil user dari JWT
            var emailLogin = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(x => x.Email == emailLogin);
            if (user == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userId = user.UserActiveId;

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                // =========================
                // 1) Ambil header yang akan diupdate
                // =========================
                var header = await _applicationDbContext.AlatPemakaians
                    .FirstOrDefaultAsync(x => x.PemakaianAlatId == pemakaianAlatId);

                if (header == null)
                    return NotFound(new { message = "Data Alat Pemakaian tidak ditemukan." });

                // Optional: validasi kunjungan tidak berubah (kalau mau dikunci)
                // if (header.KunjunganId != vm.KunjunganId.Value)
                //     return BadRequest(new { message = "KunjunganId tidak boleh diubah." });

                // =========================
                // 2) Update HEADER
                // =========================
                header.KunjunganId = vm.KunjunganId.Value;
                header.PasienId = vm.PasienId.Value;
                header.TanggalPemakaian = vm.TanggalPemakaian ?? header.TanggalPemakaian; // atau DateTime.UtcNow
                header.Keterangan = vm.Keterangan;

                // kalau punya kolom update audit:
                // header.UpdateBy = userId;
                // header.UpdateDateTime = DateTimeOffset.UtcNow;

                _applicationDbContext.AlatPemakaians.Update(header);

                // =========================
                // 3) Hapus DETAIL lama (replace all)
                // =========================
                var oldDetails = await _applicationDbContext.AlatPemakaianDetails
                    .Where(d => d.PemakaianAlatId == pemakaianAlatId)
                    .ToListAsync();

                if (oldDetails.Count > 0)
                    _applicationDbContext.AlatPemakaianDetails.RemoveRange(oldDetails);

                // =========================
                // 4) Hapus BILLING Alkes untuk kunjungan ini (replace all)
                //    Catatan: jika billing alkes bisa berasal dari modul lain,
                //    lebih aman pakai marker / reference id. Tapi untuk sekarang,
                //    kita asumsikan billing alkes untuk kunjungan ini berasal dari pemakaian alat.
                // =========================
                var oldBillings = await _applicationDbContext.Billings
                    .Where(b => b.KunjunganId == vm.KunjunganId.Value &&
                                b.JenisBilling != null &&
                                b.JenisBilling.ToLower() == "alkes")
                    .ToListAsync();

                if (oldBillings.Count > 0)
                    _applicationDbContext.Billings.RemoveRange(oldBillings);

                // =========================
                // 5) Siapkan billingIndex baru (lanjut nomor dari billing existing non-alkes)
                //    atau tetap khusus alkes saja seperti POST kamu:
                // =========================
                int billingIndex = await _applicationDbContext.Billings
                    .CountAsync(b => b.KunjunganId == vm.KunjunganId.Value &&
                                     b.JenisBilling != null &&
                                     b.JenisBilling.ToLower() == "alkes");

                // =========================
                // 6) Insert DETAIL baru + BILLING baru (aggregate per alat)
                // =========================
                var billingDict = new Dictionary<Guid, Billing>(); // key: PeralatanId
                var detailEntities = new List<AlatPemakaianDetail>();

                foreach (var d in vm.Details)
                {
                    if (d?.PeralatanId == null)
                        return BadRequest(new { message = "PeralatanId pada detail wajib diisi." });

                    var alatId = d.PeralatanId.Value;

                    var qtyInput = d.QtyPemakaian ?? 1;
                    if (qtyInput <= 0) qtyInput = 1;

                    // Ambil tarif per kelas
                    //var alatDb = await _applicationDbContext.TarifKelass
                    //    .FirstOrDefaultAsync(x => x.PeralatanId == alatId && x.KelasId == d.KelasId);

                    var namaAlat = await _applicationDbContext.Peralatans
                        .Where(x => x.PeralatanId == alatId)
                        .Select(x => x.NamaPeralatan)
                        .FirstOrDefaultAsync();

                    //if (alatDb == null && namaAlat == null)
                    //    return BadRequest(new { message = $"Peralatan tidak ditemukan: {alatId}" });

                    //if (alatDb == null)
                    //    return BadRequest(new { message = $"Tarif alat belum tersedia untuk PeralatanId {alatId} dan KelasId {d.KelasId}." });

                    //var subTotal = alatDb.TarifRs * qtyInput;

                    var coverage = await _asuransiCoverageService.ResolveCoverageAsync(
                        vm.KunjunganId,
                        "Alkes",
                        alatId,
                        ct);


                    // ---- DETAIL ----
                    detailEntities.Add(new AlatPemakaianDetail
                    {
                        DetailPemakaianAlatId = Guid.NewGuid(),
                        PemakaianAlatId = pemakaianAlatId,
                        PeralatanId = alatId,
                        KelasId = d.KelasId,
                        QtyPemakaian = qtyInput,
                        //HargaPeralatan = alatDb.TarifRs,
                        //TotalPemakaianAlat = subTotal,
                        Keterangan = d.Keterangan,
                        CreateBy = userId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    });

                    // ---- BILLING (aggregate per alat) ----
                    if (!billingDict.TryGetValue(alatId, out var billing))
                    {
                        billingIndex++;

                        billing = new Billing
                        {
                            BillingId = Guid.NewGuid(),
                            KunjunganId = vm.KunjunganId.Value,

                            BillingDate = DateTime.UtcNow,
                            BillingKode = $"{billingIndex:D3}",
                            InvoiceBilling = await _generateInvoiceBillingService.GetOrCreateAsync(
                                (Guid)header.KunjunganId,
                                DateTime.UtcNow),
                            IsListWhiteOff = false,
                            ItemId = alatId,
                            NamaItem = namaAlat,
                            LayananId = vm.LayananId,

                            //HargaItem = alatDb.TarifRs,
                            QtyItem = qtyInput,
                            //SubTotalItem = alatDb.TarifRs * qtyInput,
                            TanggalInvoice = DateTime.UtcNow,
                            TanggalJatuhTempo = DateTime.UtcNow.Date.AddDays(90),
                            JenisBilling = "Alkes",
                            StatusPengambilan = true,
                            StatusBilling = false,

                            IsCovered = coverage?.IsCovered,
                            IsCoveredExcess = coverage?.IsCoveredExcess,
                            AsuransiId = coverage?.AsuransiId,
                            AsuransiExcessId = coverage?.AsuransiExcessId,
                            CreateBy = userId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        billingDict[alatId] = billing;
                    }
                    else
                    {
                        billing.QtyItem += qtyInput;
                        billing.SubTotalItem = billing.HargaItem * billing.QtyItem;
                    }
                }

                _applicationDbContext.AlatPemakaianDetails.AddRange(detailEntities);
                _applicationDbContext.Billings.AddRange(billingDict.Values);

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                await _hubContext.Clients.All.SendAsync("Pemakaian alat diupdate", new
                {
                    action = "update",
                    pemakaianAlatId
                });

                return Ok(new
                {
                    message = "Berhasil update Alat Pemakaian + Detail + Billing",
                    pemakaianAlatId,
                    totalDetail = detailEntities.Count,
                    totalBilling = billingDict.Count
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Auth
            var emailLogin = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(emailLogin))
                return Unauthorized(new { message = "User tidak terautentikasi!" });

            var user = await _applicationDbContext.UserActives.FirstOrDefaultAsync(x => x.Email == emailLogin);
            if (user == null)
                return Unauthorized(new { message = "User aktif tidak ditemukan!" });

            var userId = user.UserActiveId;

            await using var trx = await _applicationDbContext.Database.BeginTransactionAsync();
            try
            {
                // =========================
                // 1) Ambil header (yang belum di-delete)
                // =========================
                var header = await _applicationDbContext.AlatPemakaians
                    .FirstOrDefaultAsync(x =>
                        x.PemakaianAlatId == id &&
                        (x.IsDelete == false || x.IsDelete == null));

                if (header == null)
                    return NotFound(new { message = "Data pemakaian alat tidak ditemukan atau sudah dihapus." });

                var kunjunganId = header.KunjunganId;

                // =========================
                // 2) Ambil details (yang belum di-delete)
                // =========================
                var details = await _applicationDbContext.AlatPemakaianDetails
                    .Where(x =>
                        x.PemakaianAlatId == id &&
                        (x.IsDelete == false || x.IsDelete == null))
                    .ToListAsync();

                var peralatanIds = details
                    .Where(d => d.PeralatanId != null)
                    .Select(d => d.PeralatanId!.Value)
                    .Distinct()
                    .ToList();

                // =========================
                // 3) Soft delete BILLING terkait
                // =========================
                // Hanya billing alkes, kunjungan sama, itemId ada di peralatanIds
                var billings = new List<Billing>();

                if (peralatanIds.Count > 0)
                {
                    billings = await _applicationDbContext.Billings
                        .Where(b =>
                            b.KunjunganId == kunjunganId &&
                            b.JenisBilling.ToLower() == "alkes" &&
                            peralatanIds.Contains((Guid)b.ItemId) &&
                            (b.IsDelete == false || b.IsDelete == null))
                        .ToListAsync();

                    foreach (var b in billings)
                    {
                        b.IsDelete = true;
                        b.DeleteBy = userId;
                        b.DeleteDateTime = DateTimeOffset.UtcNow;

                        // kalau kolom delete tidak ada, pakai update:
                        // b.UpdateBy = userId;
                        // b.UpdateDateTime = DateTimeOffset.UtcNow;
                    }
                }

                // =========================
                // 4) Soft delete DETAILS
                // =========================
                foreach (var d in details)
                {
                    d.IsDelete = true;
                    d.DeleteBy = userId;
                    d.DeleteDateTime = DateTimeOffset.UtcNow;

                    // jika tidak ada kolom Delete*, pakai Update*
                    // d.UpdateBy = userId;
                    // d.UpdateDateTime = DateTimeOffset.UtcNow;
                }

                // =========================
                // 5) Soft delete HEADER
                // =========================
                header.IsDelete = true;
                header.DeleteBy = userId;
                header.DeleteDateTime = DateTimeOffset.UtcNow;

                // kalau tidak ada kolom delete, pakai update:
                // header.UpdateBy = userId;
                // header.UpdateDateTime = DateTimeOffset.UtcNow;

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new
                {
                    message = "Berhasil soft delete pemakaian alat + detail + billing terkait.",
                    pemakaianAlatId = id,
                    deletedDetails = details.Count,
                    deletedBillings = billings.Count
                });
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                return StatusCode(500, new { message = $"Terjadi kesalahan internal: {ex.Message}" });
            }
        }


        [HttpGet("paged")]
        public async Task<IActionResult> PagedAlatPemakaian(
            int page = 1,
            int perPage = 10,
            Guid? kunjunganId = null,

            string? orderBy = "CreateDateTime",
            string? sortDirection = "desc",

            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? startDate = null,

            [FromQuery, SwaggerSchema(Format = "date-time", Description = "Format: YYYY-MM-DD")]
            DateTime? endDate = null,

            [FromQuery, JsonConverter(typeof(StringEnumConverter))] PeriodeFilter? periode = null
        )
        {
            if (page < 1) page = 1;
            if (perPage < 1) perPage = 10;

            // =========================
            // Base Query (headers)
            // =========================
            var query =
                from a in _applicationDbContext.AlatPemakaians
                join u0 in _applicationDbContext.UserActives on a.CreateBy equals u0.UserActiveId into uu
                from u in uu.DefaultIfEmpty()
                where a.IsDelete == false || a.IsDelete == null
                select new
                {
                    a.PemakaianAlatId,
                    a.KunjunganId,
                    a.PasienId,
                    a.TanggalPemakaian,
                    a.Keterangan,
                    a.CreateDateTime,
                    a.CreateBy,
                    CreateByName = u != null ? u.FullName : null
                };

            // =========================
            // Filter by kunjunganId
            // =========================
            if (kunjunganId.HasValue)
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);

            // =========================
            // Date range
            // =========================
            if (startDate.HasValue && endDate.HasValue)
            {
                DateTimeOffset startUtc = startDate.Value.Date.ToUniversalTime();
                DateTimeOffset endUtc = endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime <= endUtc);
            }

            // =========================
            // Periode
            // =========================
            if (periode.HasValue)
            {
                var todayUtc = DateTime.UtcNow.Date;

                switch (periode.Value)
                {
                    case PeriodeFilter.Today:
                        query = query.Where(x => x.CreateDateTime.Date == todayUtc);
                        break;

                    case PeriodeFilter.ThisWeek:
                        var startOfWeek = todayUtc.AddDays(-(int)todayUtc.DayOfWeek);
                        query = query.Where(x => x.CreateDateTime.Date >= startOfWeek && x.CreateDateTime.Date <= todayUtc);
                        break;

                    case PeriodeFilter.LastWeek:
                        var startOfThisWeek = todayUtc.AddDays(-(int)todayUtc.DayOfWeek);
                        var startOfLastWeek = startOfThisWeek.AddDays(-7);
                        query = query.Where(x => x.CreateDateTime.Date >= startOfLastWeek && x.CreateDateTime.Date < startOfThisWeek);
                        break;

                    case PeriodeFilter.ThisMonth:
                        query = query.Where(x => x.CreateDateTime.Month == todayUtc.Month && x.CreateDateTime.Year == todayUtc.Year);
                        break;

                    case PeriodeFilter.LastMonth:
                        var lm = todayUtc.AddMonths(-1);
                        query = query.Where(x => x.CreateDateTime.Month == lm.Month && x.CreateDateTime.Year == lm.Year);
                        break;

                    case PeriodeFilter.ThisYear:
                        query = query.Where(x => x.CreateDateTime.Year == todayUtc.Year);
                        break;

                    case PeriodeFilter.LastYear:
                        query = query.Where(x => x.CreateDateTime.Year == todayUtc.Year - 1);
                        break;

                    case PeriodeFilter.Last3Months:
                        query = query.Where(x => x.CreateDateTime >= todayUtc.AddMonths(-3));
                        break;

                    case PeriodeFilter.Last6Months:
                        query = query.Where(x => x.CreateDateTime >= todayUtc.AddMonths(-6));
                        break;
                }
            }

            // =========================
            // Sorting
            // =========================
            bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = desc
                ? orderBy switch
                {
                    "CreateDateTime" => query.OrderByDescending(x => x.CreateDateTime),
                    "TanggalPemakaian" => query.OrderByDescending(x => x.TanggalPemakaian),
                    "CreateByName" => query.OrderByDescending(x => x.CreateByName),
                    _ => query.OrderByDescending(x => x.CreateDateTime)
                }
                : orderBy switch
                {
                    "CreateDateTime" => query.OrderBy(x => x.CreateDateTime),
                    "TanggalPemakaian" => query.OrderBy(x => x.TanggalPemakaian),
                    "CreateByName" => query.OrderBy(x => x.CreateByName),
                    _ => query.OrderBy(x => x.CreateDateTime)
                };

            // =========================
            // Pagination counts
            // =========================
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
                return NotFound(new { message = "Data tidak ditemukan." });

            if (page > totalPages)
                return NotFound(new { message = "Page not found." });

            // =========================
            // Fetch page headers
            // =========================
            var pageHeaders = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();

            var pemakaianIds = pageHeaders.Select(h => h.PemakaianAlatId).ToList();

            // =========================
            // Fetch details for those headers (ONE query)
            // + join master Peralatan & Kelas (optional)
            // =========================
            var pageDetails = await (
                from d in _applicationDbContext.AlatPemakaianDetails
                where pemakaianIds.Contains((Guid)d.PemakaianAlatId) && !d.IsDelete

                join p0 in _applicationDbContext.Peralatans on d.PeralatanId equals p0.PeralatanId into pp
                from p in pp.DefaultIfEmpty()

                join k0 in _applicationDbContext.Kelass on d.KelasId equals k0.KelasId into kk
                from kls in kk.DefaultIfEmpty()

                select new
                {
                    d.PemakaianAlatId,
                    d.DetailPemakaianAlatId,
                    d.PeralatanId,
                    NamaPeralatan = p != null ? p.NamaPeralatan : null,
                    d.KelasId,
                    NamaKelas = kls != null ? kls.NamaKelas : null,
                    d.QtyPemakaian,
                    d.HargaPeralatan,
                    d.TotalPemakaianAlat,
                    d.Keterangan,
                    d.IsDelete,
                    d.CreateDateTime,
                }
            ).ToListAsync();

            // ✅ ILookup: kalau key tidak ada, hasilnya empty (bukan null)
            var detailLookup = pageDetails.ToLookup(x => x.PemakaianAlatId);

            // =========================
            // Compose response rows (no ternary type issue)
            // =========================
            var rows = pageHeaders.Select(h => new
            {
                h.PemakaianAlatId,
                h.KunjunganId,
                h.PasienId,
                h.TanggalPemakaian,
                h.Keterangan,
                h.CreateDateTime,
                h.CreateBy,
                h.CreateByName,
                Details = detailLookup[h.PemakaianAlatId].ToList()
            });

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
