using System.Linq;
using System.Security.Claims;
using System.Threading;
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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Interfaces;
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
    [EnableCors("FrontendCorsPolicy")]
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
        private readonly IKunjunganTransactionGuard _kunjunganTransactionGuard;
        public AlatPemakaianController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AlatPemakaianController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<AlatPemakaianHub> hubContext,
            IGenerateInvoiceBillingService generateInvoiceBillingService,
            IKunjunganTransactionGuard kunjunganTransactionGuard,
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
            _kunjunganTransactionGuard = kunjunganTransactionGuard;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _applicationDbContext.AlatPemakaians
                .AsNoTracking()
                .Where(x => x.PemakaianAlatId == id && (x.IsDelete == false || x.IsDelete == null))
                .Select(x => new
                {
                    Header = new
                    {
                        x.PemakaianAlatId,
                        x.KunjunganId,
                        x.PasienId,
                        x.TanggalPemakaian,
                        x.Keterangan,
                        x.CreateDateTime,
                        x.CreateBy
                    },
                    Details = x.Details
                        .Where(d => d.IsDelete == false || d.IsDelete == null)
                        .Select(d => new
                        {
                            d.DetailPemakaianAlatId,
                            d.PeralatanId,
                            NamaPeralatan = d.Peralatan != null ? d.Peralatan.NamaPeralatan : null,
                            d.KelasId,
                            NamaKelas = d.Kelas != null ? d.Kelas.NamaKelas : null,
                            d.QtyPemakaian,
                            d.HargaPeralatan,
                            d.TotalPemakaianAlat,
                            d.Keterangan
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(new { message = "Data pemakaian alat tidak ditemukan." });
            }

            return Ok(result);
        }


        [HttpGet("by-kunjungan/{kunjunganId}")]
        public async Task<IActionResult> GetByKunjunganId(Guid kunjunganId)
        {
            var data = await _applicationDbContext.AlatPemakaians
                .AsNoTracking()
                .Where(x => x.KunjunganId == kunjunganId && (x.IsDelete == false || x.IsDelete == null))
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new
                {
                    Header = new
                    {
                        x.PemakaianAlatId,
                        x.KunjunganId,
                        x.PasienId,
                        x.TanggalPemakaian,
                        x.Keterangan,
                        x.CreateDateTime
                    },
                    Details = x.Details
                        .Where(d => d.IsDelete == false || d.IsDelete == null)
                        .Select(d => new
                        {
                            d.DetailPemakaianAlatId,
                            d.PemakaianAlatId,
                            d.PeralatanId,
                            NamaPeralatan = d.Peralatan != null ? d.Peralatan.NamaPeralatan : null,
                            d.KelasId,
                            NamaKelas = d.Kelas != null ? d.Kelas.NamaKelas : null,
                            d.QtyPemakaian,
                            d.HargaPeralatan,
                            d.TotalPemakaianAlat,
                            d.Keterangan
                        })
                        .ToList()
                })
                .ToListAsync();

            if (data.Count == 0)
            {
                return NotFound(new { message = "Tidak ada data pemakaian alat untuk kunjungan ini." });
            }

            return Ok(new
            {
                KunjunganId = kunjunganId,
                TotalPemakaian = data.Count,
                Data = data
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
            await _kunjunganTransactionGuard.EnsureCanAddTransactionAsync((Guid)vm.KunjunganId,ct);

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
            await _kunjunganTransactionGuard.EnsureCanAddTransactionAsync((Guid)vm.KunjunganId, ct);
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
            Guid? pasienId = null,
            string? namaAlat = null,

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

            var namaAlatPattern = !string.IsNullOrWhiteSpace(namaAlat)
                ? $"%{namaAlat.Trim()}%"
                : null;

            // =========================================
            // Base query
            // =========================================
            var query = _applicationDbContext.AlatPemakaians
                .AsNoTracking()
                .Where(x => x.IsDelete == false || x.IsDelete == null);

            // =========================================
            // Filter: KunjunganId
            // =========================================
            if (kunjunganId.HasValue)
            {
                query = query.Where(x => x.KunjunganId == kunjunganId.Value);
            }

            // =========================================
            // Filter: PasienId
            // =========================================
            if (pasienId.HasValue)
            {
                query = query.Where(x => x.PasienId == pasienId.Value);
            }

            // =========================================
            // Filter: Nama Alat
            // Cari di tabel detail -> peralatan
            // PostgreSQL: ILike = case-insensitive
            // =========================================
            if (!string.IsNullOrWhiteSpace(namaAlatPattern))
            {
                query = query.Where(x => x.Details.Any(d =>
                    (d.IsDelete == false || d.IsDelete == null) &&
                    d.Peralatan != null &&
                    d.Peralatan.NamaPeralatan != null &&
                    EF.Functions.ILike(d.Peralatan.NamaPeralatan, namaAlatPattern)));
            }

            // =========================================
            // Date range
            // =========================================
            if (startDate.HasValue && endDate.HasValue)
            {
                var startUtc = new DateTimeOffset(startDate.Value.Date.ToUniversalTime());
                var endUtc = new DateTimeOffset(endDate.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime());

                query = query.Where(x => x.CreateDateTime >= startUtc && x.CreateDateTime <= endUtc);
            }

            // =========================================
            // Periode
            // =========================================
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
                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.ThisWeek:
                        start = today.AddDays(-(int)today.DayOfWeek);
                        endExclusive = today.AddDays(1);
                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.LastWeek:
                        var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek);
                        start = startOfThisWeek.AddDays(-7);
                        endExclusive = startOfThisWeek;
                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.ThisMonth:
                        start = new DateTime(today.Year, today.Month, 1);
                        endExclusive = start.AddMonths(1);
                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.LastMonth:
                        var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
                        start = firstDayThisMonth.AddMonths(-1);
                        endExclusive = firstDayThisMonth;
                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.ThisYear:
                        start = new DateTime(today.Year, 1, 1);
                        endExclusive = start.AddYears(1);
                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.LastYear:
                        start = new DateTime(today.Year - 1, 1, 1);
                        endExclusive = new DateTime(today.Year, 1, 1);
                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.Last3Months:
                        start = today.AddMonths(-3);
                        endExclusive = today.AddDays(1);
                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;

                    case PeriodeFilter.Last6Months:
                        start = today.AddMonths(-6);
                        endExclusive = today.AddDays(1);
                        query = query.Where(x => x.CreateDateTime >= start && x.CreateDateTime < endExclusive);
                        break;
                }
            }

            // =========================================
            // Sorting
            // =========================================
            bool desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = (orderBy ?? "CreateDateTime").Trim() switch
            {
                "TanggalPemakaian" => desc
                    ? query.OrderByDescending(x => x.TanggalPemakaian)
                    : query.OrderBy(x => x.TanggalPemakaian),

                "CreateByName" => desc
                    ? query.OrderByDescending(x =>
                        _applicationDbContext.UserActives
                            .Where(u => u.UserActiveId == x.CreateBy)
                            .Select(u => u.FullName)
                            .FirstOrDefault())
                    : query.OrderBy(x =>
                        _applicationDbContext.UserActives
                            .Where(u => u.UserActiveId == x.CreateBy)
                            .Select(u => u.FullName)
                            .FirstOrDefault()),

                _ => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };

            // =========================================
            // Count
            // =========================================
            var totalRows = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRows / (double)perPage);

            if (totalRows == 0)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            if (page > totalPages)
            {
                return NotFound(new { message = "Page not found." });
            }

            // =========================================
            // Fetch paged data + details via navigation
            // =========================================
            var rows = await query
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(x => new
                {
                    x.PemakaianAlatId,
                    x.KunjunganId,
                    x.PasienId,
                    x.TanggalPemakaian,
                    x.Keterangan,
                    x.CreateDateTime,
                    x.CreateBy,

                    CreateByName = _applicationDbContext.UserActives
                        .Where(u => u.UserActiveId == x.CreateBy)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    Details = x.Details
                        .Where(d =>
                            (d.IsDelete == false || d.IsDelete == null) &&
                            (
                                namaAlatPattern == null ||
                                (d.Peralatan != null &&
                                 d.Peralatan.NamaPeralatan != null &&
                                 EF.Functions.ILike(d.Peralatan.NamaPeralatan, namaAlatPattern))
                            ))
                        .Select(d => new
                        {
                            d.DetailPemakaianAlatId,
                            d.PemakaianAlatId,
                            d.PeralatanId,
                            NamaPeralatan = d.Peralatan != null ? d.Peralatan.NamaPeralatan : null,
                            d.KelasId,
                            NamaKelas = d.Kelas != null ? d.Kelas.NamaKelas : null,
                            d.QtyPemakaian,
                            d.HargaPeralatan,
                            d.TotalPemakaianAlat,
                            d.Keterangan,
                            d.CreateDateTime
                        })
                        .ToList()
                })
                .ToListAsync();

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
