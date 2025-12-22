using System.Linq;
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
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Pendaftaran.Enum;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // =========================
            // Ambil header
            // =========================
            var header = await _applicationDbContext.AlatPemakaians
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
            // Ambil detail alat
            // =========================
            var details = await _applicationDbContext.AlatPemakaianDetails
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
            // Ambil nama alat & kelas (optional, tapi biasanya dibutuhkan frontend)
            // =========================
            var alatIds = details.Select(d => d.PeralatanId).Distinct().ToList();
            var kelasIds = details.Select(d => d.KelasId).Distinct().ToList();

            var namaAlatDict = await _applicationDbContext.Peralatans
                .Where(x => alatIds.Contains(x.PeralatanId))
                .Select(x => new { x.PeralatanId, x.NamaPeralatan })
                .ToDictionaryAsync(x => x.PeralatanId, x => x.NamaPeralatan);

            var kelasDict = await _applicationDbContext.Kelass
                .Where(x => kelasIds.Contains(x.KelasId))
                .Select(x => new { x.KelasId, x.NamaKelas })
                .ToDictionaryAsync(x => x.KelasId, x => x.NamaKelas);

            //// =========================
            //// Billing alkes untuk kunjungan ini (optional)
            //// =========================
            //var billings = await _applicationDbContext.Billings
            //    .Where(b =>
            //        b.KunjunganId == header.KunjunganId &&
            //        b.JenisBilling.ToLower() == "alkes" &&
            //        alatIds.Contains(b.ItemId))
            //    .Select(b => new
            //    {
            //        b.BillingId,
            //        b.BillingKode,
            //        b.ItemId,
            //        b.NamaItem,
            //        b.QtyItem,
            //        b.HargaItem,
            //        b.SubTotalItem
            //    })
            //    .ToListAsync();

            // =========================
            // Final response
            // =========================
            var result = new
            {
                Header = header,
                Details = details.Select(d => new
                {
                    d.DetailPemakaianAlatId,
                    d.PeralatanId,
                    NamaPeralatan = namaAlatDict.TryGetValue((Guid)d.PeralatanId, out var nama) ? nama : null,
                    d.KelasId,
                    NamaKelas = kelasDict.TryGetValue((Guid)d.KelasId, out var kelas) ? kelas : null,
                    d.QtyPemakaian,
                    d.HargaPeralatan,
                    d.TotalPemakaianAlat,
                    d.Keterangan
                }),
                //Billings = billings
            };

            return Ok(result);
        }

        [HttpGet("by-kunjungan/{kunjunganId}")]
        public async Task<IActionResult> GetByKunjunganId(Guid kunjunganId)
        {
            // =========================
            // Ambil semua header pemakaian alat
            // =========================
            var headers = await _applicationDbContext.AlatPemakaians
                .Where(x => x.KunjunganId == kunjunganId)
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
            // Ambil semua detail
            // =========================
            var details = await _applicationDbContext.AlatPemakaianDetails
                .Where(x => pemakaianIds.Contains((Guid)x.PemakaianAlatId))
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
            // Ambil master alat & kelas
            // =========================
            var alatIds = details.Select(d => d.PeralatanId).Distinct().ToList();
            var kelasIds = details.Select(d => d.KelasId).Distinct().ToList();

            var namaAlatDict = await _applicationDbContext.Peralatans
                .Where(x => alatIds.Contains(x.PeralatanId))
                .Select(x => new { x.PeralatanId, x.NamaPeralatan })
                .ToDictionaryAsync(x => x.PeralatanId, x => x.NamaPeralatan);

            var kelasDict = await _applicationDbContext.Kelass
                .Where(x => kelasIds.Contains(x.KelasId))
                .Select(x => new { x.KelasId, x.NamaKelas })
                .ToDictionaryAsync(x => x.KelasId, x => x.NamaKelas);

            // =========================
            // Billing alkes (per kunjungan)
            // =========================
            //var billings = await _applicationDbContext.Billings
            //    .Where(b => b.KunjunganId == kunjunganId && b.JenisBilling.ToLower() == "alkes")
            //    .Select(b => new
            //    {
            //        b.BillingId,
            //        b.BillingKode,
            //        b.ItemId,
            //        b.NamaItem,
            //        b.QtyItem,
            //        b.HargaItem,
            //        b.SubTotalItem
            //    })
            //    .ToListAsync();

            // =========================
            // Group response per pemakaian alat
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
                        NamaPeralatan = namaAlatDict.TryGetValue((Guid)d.PeralatanId, out var nama) ? nama : null,
                        d.KelasId,
                        NamaKelas = kelasDict.TryGetValue((Guid)d.KelasId, out var kelas) ? kelas : null,
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
                Data = result,
                //Billings = billings
            });
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
                    var subTotal = (alatDb.TarifRs * qtyInput);

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


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAlatPemakaian(Guid id, [FromBody] AlatPemakaianViewModel vm)
        {
            if (vm == null || !ModelState.IsValid)
                return BadRequest(new { message = "Data tidak valid." });

            if (vm.KunjunganId == null || vm.PasienId == null)
                return BadRequest(new { message = "KunjunganId dan PasienId wajib diisi." });

            if (vm.Details == null || vm.Details.Count == 0)
                return BadRequest(new { message = "Detail pemakaian alat wajib diisi minimal 1 item." });

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
                // 1) Ambil header
                // =========================
                var header = await _applicationDbContext.AlatPemakaians
                    .FirstOrDefaultAsync(x => x.PemakaianAlatId == id);

                if (header == null)
                    return NotFound(new { message = "Data pemakaian alat tidak ditemukan." });

                // =========================
                // 2) Ambil detail existing pakai DbSet detail (tanpa navigation)
                // =========================
                var existingDetails = await _applicationDbContext.AlatPemakaianDetails
                    .Where(x => x.PemakaianAlatId == id)
                    .ToListAsync();

                // Update header
                header.KunjunganId = vm.KunjunganId.Value;
                header.PasienId = vm.PasienId.Value;
                header.TanggalPemakaian = vm.TanggalPemakaian ?? header.TanggalPemakaian;
                header.Keterangan = vm.Keterangan;
                header.UpdateBy = userId;
                header.UpdateDateTime = DateTimeOffset.UtcNow;

                // =========================
                // 3) Preload nama alat + tarif (hindari query per loop)
                // =========================
                var alatIds = vm.Details.Where(d => d?.PeralatanId != null).Select(d => d!.PeralatanId!.Value).Distinct().ToList();
                var kelasIds = vm.Details.Where(d => d?.KelasId != null).Select(d => d!.KelasId!.Value).Distinct().ToList();

                var namaAlatDict = await _applicationDbContext.Peralatans
                    .Where(x => alatIds.Contains(x.PeralatanId))
                    .Select(x => new { x.PeralatanId, x.NamaPeralatan })
                    .ToDictionaryAsync(x => x.PeralatanId, x => x.NamaPeralatan);

                var tarifDict = await _applicationDbContext.TarifKelass
                    .Where(x => alatIds.Contains((Guid)x.PeralatanId) && kelasIds.Contains((Guid)x.KelasId))
                    .Select(x => new { x.PeralatanId, x.KelasId, x.TarifRs })
                    .ToDictionaryAsync(x => (x.PeralatanId, x.KelasId), x => x.TarifRs);

                // =========================
                // 4) Billing Index + billing existing (tanpa PemakaianAlatId)
                // =========================
                int billingIndex = await _applicationDbContext.Billings
                    .CountAsync(b => b.KunjunganId == vm.KunjunganId.Value && b.JenisBilling.ToLower() == "alkes");

                // Billing per item per kunjungan
                var billingDict = await _applicationDbContext.Billings
                    .Where(b => b.KunjunganId == vm.KunjunganId.Value && b.JenisBilling.ToLower() == "alkes")
                    .ToDictionaryAsync(b => b.ItemId); // key: PeralatanId

                // =========================
                // 5) Update/Add detail + billing
                // =========================
                foreach (var d in vm.Details)
                {
                    if (d?.PeralatanId == null || d.KelasId == null)
                        return BadRequest(new { message = "PeralatanId dan KelasId wajib diisi pada detail." });

                    var alatId = d.PeralatanId.Value;
                    var kelasId = d.KelasId.Value;

                    if (!tarifDict.TryGetValue((alatId, kelasId), out var tarifRs))
                        return BadRequest(new { message = $"Tarif tidak ditemukan untuk PeralatanId={alatId}, KelasId={kelasId}" });

                    var qty = d.QtyPemakaian ?? 1;
                    if (qty <= 0) qty = 1;

                    var subTotal = (tarifRs * qty);

                    // --- Update detail jika ada id detail ---
                    AlatPemakaianDetail? existingDetail = null;
                    if (d.DetailPemakaianAlatId.HasValue)
                    {
                        existingDetail = existingDetails.FirstOrDefault(x => x.DetailPemakaianAlatId == d.DetailPemakaianAlatId.Value);
                    }

                    if (existingDetail != null)
                    {
                        existingDetail.PeralatanId = alatId;
                        existingDetail.KelasId = kelasId;
                        existingDetail.QtyPemakaian = qty;
                        existingDetail.HargaPeralatan = tarifRs;
                        existingDetail.TotalPemakaianAlat = subTotal;
                        existingDetail.Keterangan = d.Keterangan;
                        existingDetail.UpdateBy = userId;
                        existingDetail.UpdateDateTime = DateTimeOffset.UtcNow;
                    }
                    else
                    {
                        // --- Tambah detail baru ---
                        var newDetail = new AlatPemakaianDetail
                        {
                            DetailPemakaianAlatId = Guid.NewGuid(),
                            PemakaianAlatId = id,
                            PeralatanId = alatId,
                            KelasId = kelasId,
                            QtyPemakaian = qty,
                            HargaPeralatan = tarifRs,
                            TotalPemakaianAlat = subTotal,
                            Keterangan = d.Keterangan,
                            CreateBy = userId,
                            CreateDateTime = DateTimeOffset.UtcNow
                        };

                        _applicationDbContext.AlatPemakaianDetails.Add(newDetail);
                        existingDetails.Add(newDetail);
                    }

                    // --- Billing update/add (tanpa reference) ---
                    namaAlatDict.TryGetValue(alatId, out var namaAlat);
                    namaAlat ??= "Pemakaian Alat";

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
                            HargaItem = tarifRs,
                            QtyItem = qty,
                            SubTotalItem = tarifRs * qty,
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
                        billing.HargaItem = tarifRs;
                        billing.QtyItem = qty; // set sesuai input terbaru (kalau mau akumulasi: +=)
                        billing.SubTotalItem = billing.HargaItem * billing.QtyItem;
                        billing.UpdateBy = userId;
                        billing.UpdateDateTime = DateTimeOffset.UtcNow;
                    }
                }

                await _applicationDbContext.SaveChangesAsync();
                await trx.CommitAsync();

                return Ok(new
                {
                    message = "Berhasil update pemakaian alat (detail tidak hilang) + billing ter-update",
                    pemakaianAlatId = id,
                    totalDetailExisting = existingDetails.Count,
                    totalBillingForKunjungan = billingDict.Count
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

            // optional search untuk header (keterangan / createByName)
            //string? search = null,

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
            // Search (header fields only)
            // =========================
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    var like = $"%{search.ToLower()}%";
            //    query = query.Where(x =>
            //        EF.Functions.ILike(x.Keterangan ?? string.Empty, like) ||
            //        EF.Functions.ILike(x.CreateByName ?? string.Empty, like)
            //    );
            //}

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
                where pemakaianIds.Contains((Guid)d.PemakaianAlatId)

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
                    d.Keterangan
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
