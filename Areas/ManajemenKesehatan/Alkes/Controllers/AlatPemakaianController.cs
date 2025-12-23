using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Hubs;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Alkes.ViewModels;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.Models;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.HubSignalR;
using QuilvianSystemBackendDev.Interfaces;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;
using QuilvianSystemBackendDev.Services;

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
        public AlatPemakaianController(
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AlatPemakaianController> logger,
            IWebHostEnvironment webHostEnvironment,
            IHubContext<AlatPemakaianHub> hubContext)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _hubContext = hubContext;
        }


        [HttpPost]
        public async Task<IActionResult> CreateAlatPemakaian([FromBody] AlatPemakaianViewModel vm)
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
                    var alatDb = await _applicationDbContext.TarifKelass
                        .Where(x => x.PeralatanId == alatId && x.KelasId == d.KelasId )
                        .FirstOrDefaultAsync();

                    var namaAlat = await _applicationDbContext.Peralatans
                        .Where(x=>x.PeralatanId == alatId)
                        .Select(x=>x.NamaPeralatan)
                        .FirstOrDefaultAsync();

                    if (alatDb == null && namaAlat==null)
                        return BadRequest(new { message = $"Peralatan tidak ditemukan: {alatId}" });

                    //var harga = d.HargaPeralatan ?? alatDb.Harga;
                    var subTotal = d.TotalPemakaianAlat ?? (alatDb.TarifRs * qtyInput);

                    // ---- DETAIL ----
                    detailEntities.Add(new AlatPemakaianDetail
                    {
                        DetailPemakaianAlatId = Guid.NewGuid(),
                        PemakaianAlatId = alatPemakaianId,
                        PeralatanId = alatId,
                        KelasId = d.KelasId,
                        QtyPemakaian = qtyInput,
                        HargaPeralatan = alatDb.TarifRs,
                        TotalPemakaianAlat = subTotal,
                        Keterangan = d.Keterangan,
                        CreateBy = userId,
                        CreateDateTime = DateTimeOffset.UtcNow
                    });

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

                            ItemId = alatId,
                            NamaItem = namaAlat,

                            HargaItem = alatDb.TarifRs,
                            QtyItem = qtyInput,
                            SubTotalItem = alatDb.TarifRs * qtyInput,

                            JenisBilling = "Alkes",
                            StatusPengambilan = true,

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

    }
}
